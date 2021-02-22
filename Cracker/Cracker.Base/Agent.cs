using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base.HashCat;
using Cracker.Base.Logging;
using Cracker.Base.Services;
using Refit;

namespace Cracker.Base
{
    public class Agent
    {
        private readonly IKrakerApi _krakerApi;
        private readonly Settings.Settings _settings;
        private readonly FiniteStateMachine _switch;
        private CancellationTokenSource? cts;
        private Task<ExecutionResult>? hashcatTask;
        private IJobHandler? jobHandler;
        private readonly IJobHandlerProvider _jobHandlerProvider;

        public Agent(Settings.Settings settings, 
            IJobHandlerProvider jobHandlerProvider)
        {
            _switch = new FiniteStateMachine(WaitJob);
            _settings = settings;
            _jobHandlerProvider = jobHandlerProvider;
            _krakerApi = RestService.For<IKrakerApi>("https://kraker.weakpass.com");
        }

        public void WaitJob()
        {
            _switch.SetStateAction(DoNothing);

            var job = _krakerApi.GetJob(_settings.Config.AgentId).Result;
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
                var hashcatPath = _settings.Config.HashCat.Path;
                Environment.CurrentDirectory = Path.GetDirectoryName(hashcatPath);
                hashcatTask = new HashCatCommandExecuter(preparationResult, _settings.Config.HashCat, _krakerApi)
                    .Execute(cts.Token);

                _switch.SetStateAction(ProcessJob);
                return;
            }

            Log.Message($"Задача от сервера не валидна: {preparationResult.Error}");
            jobHandler = null;
            hashcatTask = null;
            _switch.SetStateAction(WaitJob);
        }

        public void DoNothing()
        {
        }

        public void ProcessJob()
        {
            _switch.SetStateAction(DoNothing);

            if (!hashcatTask.IsCompleted)
            {
                var heartbeat = _krakerApi.SendAgentStatus(_settings.Config.AgentId).Result;
                if (heartbeat.Status == "cancel")
                {
                    Log.Message("The job is cancel");
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

        public void Work()
        {
            _switch.RunAction();
        }
    }
}