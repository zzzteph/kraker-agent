using System.Threading;
using System.Threading.Tasks;
using Cracker.Base.Domain.AgentId;
using Cracker.Base.Domain.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Services;
using Cracker.Base.Settings;
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
        private readonly string _workingDirectory;
        
        private CancellationTokenSource? cts;
        private Task<ExecutionResult>? hashcatTask;
        private IJobHandler? jobHandler;

        public Agent(IJobHandlerProvider jobHandlerProvider,
            IAgentIdManager _agentIdManager,
            IKrakerApi krakerApi,
            Config config,
            ILogger logger,
            IWorkingDirectoryProvider workingDirectoryProvider)
        {
            _switch = new FiniteStateMachine(WaitJob);
            _jobHandlerProvider = jobHandlerProvider;
            _krakerApi = krakerApi;
            _config = config;
            _logger = logger;
            _workingDirectory = workingDirectoryProvider.Get();
            _agentId = _agentIdManager.GetCurrent().Id;
            
        }

        public async Task WaitJob()
        {
            _switch.SetStateAction(DoNothing);

            var job = await _krakerApi.GetJob(_agentId);
            if (job == null || job is IncorrectJob or DoNothingJob)
            {
                _switch.SetStateAction(WaitJob);
                return;
            }

            jobHandler = _jobHandlerProvider.Get(job);
            var preparationResult = jobHandler.Prepare(job);
            if (preparationResult.IsReadyForExecution)
            {
                cts = new CancellationTokenSource();
                hashcatTask = new HashCatCommandExecuter(preparationResult, _config.HashCat, _logger, _workingDirectory)
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
                var heartbeat = await _krakerApi.SendAgentStatus(_agentId);
                if (heartbeat.Status == "cancel")
                {
                    _logger.Information("The job is canceled");
                    cts.Cancel();
                }

                _switch.SetStateAction(ProcessJob);
                return;
            }

            await jobHandler.Clear(hashcatTask.Result);

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