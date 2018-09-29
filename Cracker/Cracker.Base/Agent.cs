using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base.HashCat;
using Cracker.Base.HttpClient;
using Cracker.Base.HttpClient.Data;
using Cracker.Base.Logging;

namespace Cracker.Base
{
	public class Agent
	{
		private readonly FiniteStateMachine @switch;
		private JobHandler jobHandler;
		private Task<ExecutionResult> hashcatTask;
		private CancellationTokenSource cts;
		private readonly Settings.Settings settings;
		private readonly ServerClient serverClient;

		public Agent(Settings.Settings settings)
		{
			@switch = new FiniteStateMachine(WaitJob);
			this.settings = settings;
			serverClient = new ServerClient(settings.Config);
		}

		public void WaitJob()
		{
			@switch.SetStateAction(DoNothing);
			
			var job = serverClient.GetJob().Result;
			if (job?.Type == null)
			{
				@switch.SetStateAction(WaitJob);
				return;
			}

			jobHandler = JobHandler.Create(job, settings);
			var preparationResult = jobHandler.Prepare();
			if (preparationResult.IsReadyForExecution)
			{
				cts = new CancellationTokenSource();
				var hashcatPath = settings.Config.HashCatPath;
				Environment.CurrentDirectory = Path.GetDirectoryName(hashcatPath);
				hashcatTask = new HashCatCommandExecuter(preparationResult.HashCatArguments, settings.Config)
					.Execute(cts.Token, false, job.JobId);

				@switch.SetStateAction(ProcessJob);
				return;
			}

			Log.Message($"Задача от сервера не валидна: {preparationResult.Error}");
			jobHandler = null;
			hashcatTask = null;
			@switch.SetStateAction(WaitJob);
		}

		public void DoNothing() { }
		
		public void ProcessJob()
		{
			@switch.SetStateAction(DoNothing);
			
			if (!hashcatTask.IsCompleted)
			{
				//todo хэшкот может завершить работу и не сгенерировать событие Exit, тогда задача зависает, надо решить проблему!
				var heartbeat = serverClient.Heartbeat().Result;
				if (heartbeat?.Type == JobType.Stop)
				{
					Log.Message($"Ну началось, откатываемся... Задача будет отменена.");
					cts.Cancel();
				}

				@switch.SetStateAction(ProcessJob);
				return;
			}

			jobHandler.Clear(hashcatTask.Result);

			jobHandler = null;
			hashcatTask = null;
			@switch.SetStateAction(WaitJob);
		}

		public void Work()
		{
			@switch.RunAction();
		}
	}
}