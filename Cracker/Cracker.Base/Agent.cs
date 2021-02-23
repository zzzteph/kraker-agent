using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base.Domain.AgentId;
using Cracker.Base.Domain.AgentInfo;
using Cracker.Base.Domain.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Services;
using Cracker.Base.Settings;
using Refit;
using Serilog;

namespace Cracker.Base
{
    public interface IAgent
    {
        Task WaitJob();
        Task DoNothing();
        Task ProcessJob();
        Task Work();
    }

    public class Agent : IAgent
    {
        private readonly string _agentId;
        private readonly IKrakerApi _krakerApi;
        private readonly FiniteStateMachine _switch;
        private readonly IJobHandlerProvider _jobHandlerProvider;
        private readonly Config _config;
        private readonly ILogger _logger;
        
        private CancellationTokenSource? cts;
        private Task<ExecutionResult>? hashcatTask;
        private IJobHandler? jobHandler;

        public Agent(IJobHandlerProvider jobHandlerProvider,
            IAgentIdManager _agentIdManager,
            IKrakerApi krakerApi,
            Config config,
            ILogger logger)
        {
            _switch = new FiniteStateMachine(WaitJob);
            _jobHandlerProvider = jobHandlerProvider;
            _krakerApi = krakerApi;
            _config = config;
            _logger = logger;
            _agentId = _agentIdManager.GetCurrent().Value;
        }

        public async Task WaitJob()
        {
            _switch.SetStateAction(DoNothing);

            var job = await _krakerApi.GetJob(_agentId);
            if (job.Type == null)
            {
                _switch.SetStateAction(WaitJob);
                return;
            }

            jobHandler = _jobHandlerProvider.Get(job);
            var preparationResult = jobHandler.Prepare(job);
            if (preparationResult.IsReadyForExecution)
            {
                cts = new CancellationTokenSource();
                var hashcatPath = _config.HashCat.Path;
                Environment.CurrentDirectory = Path.GetDirectoryName(hashcatPath);
                hashcatTask = new HashCatCommandExecuter(preparationResult, _config.HashCat, _logger)
                    .Execute(cts.Token);

                _switch.SetStateAction(ProcessJob);
                return;
            }

            _logger.Information($"Got an invalid task: {preparationResult.Error}");
            jobHandler = null;
            hashcatTask = null;
            _switch.SetStateAction(WaitJob);
        }

        public Task DoNothing() =>Task.CompletedTask;

        public async Task ProcessJob()
        {
            _switch.SetStateAction(DoNothing);

            if (!hashcatTask.IsCompleted)
            {
                var heartbeat = _krakerApi.SendAgentStatus(_config.AgentId).Result;
                if (heartbeat.Status == "cancel")
                {
                    _logger.Information("The job is cancel");
                    cts.Cancel();
                }

                _switch.SetStateAction(ProcessJob);
                return;
            }

            jobHandler.Clear(hashcatTask.Result);

            jobHandler = null;
            hashcatTask = null;
            _switch.SetStateAction(WaitJob);
        }

        public async Task Work()
        {
            await _switch.RunAction();
        }
    }
}