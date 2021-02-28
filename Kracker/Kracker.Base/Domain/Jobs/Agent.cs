using System;
using System.Threading.Tasks;
using Kracker.Base.Domain.AgentId;
using Kracker.Base.Services;
using Kracker.Base.Services.Model.Jobs;
using Kracker.Base.Tools;
using Serilog;

namespace Kracker.Base.Domain.Jobs
{
    public interface IAgent
    {
        Task Work();
    }

    public class Agent : IAgent
    {
        private readonly string _agentId;
        private readonly IJobHandlerProvider _jobHandlerProvider;
        private readonly IKrakerApi _krakerApi;
        private readonly ILogger _logger;
        private readonly FiniteStateMachine _switch;
        private readonly IIncorrectJobHandler _incorrectJobHandler;
        private IJobHandler _jobHandler;
        

        public Agent(IJobHandlerProvider jobHandlerProvider,
            IAgentIdManager agentIdManager,
            IKrakerApi krakerApi,
            ILogger logger,
            IIncorrectJobHandler incorrectJobHandler)
        {
            _switch = new FiniteStateMachine(WaitJob);
            _jobHandlerProvider = jobHandlerProvider;
            _krakerApi = krakerApi;
            _logger = logger;
            _agentId = agentIdManager.GetCurrent().Id 
                       ?? throw new InvalidOperationException("The agent needs to have id");
            _jobHandler = incorrectJobHandler;
            _incorrectJobHandler = incorrectJobHandler;
        }

        public async Task Work()
        {
            await _switch.RunAction();
        }

        public async Task WaitJob()
        {
            _switch.SetStateAction(DoNothing);

            var job = await _krakerApi.GetJob(_agentId);
            _logger.Information("Got a job {0}", job);

            if (job == null || job is IncorrectJob or DoNothingJob)
            {
                _switch.SetStateAction(WaitJob);
                return;
            }

            _jobHandler = _jobHandlerProvider.Get(job);

            _jobHandler.Execute();
            _switch.SetStateAction(ProcessJob);
        }

        private Task DoNothing() => Task.CompletedTask;

        private async Task ProcessJob()
        {
            _switch.SetStateAction(DoNothing);

            if (!_jobHandler.IsCompleted())
            {
                var heartbeat = await _krakerApi.SendAgentStatus(_agentId, _jobHandler.GetJobDescription());
                if (heartbeat.Status == "cancel")
                {
                    _logger.Information("The job is canceled");
                    _jobHandler.Cancel();
                }

                _switch.SetStateAction(ProcessJob);
                return;
            }

            await _jobHandler.Clear();

            _jobHandler = _incorrectJobHandler;
            _switch.SetStateAction(WaitJob);
        }
    }
}