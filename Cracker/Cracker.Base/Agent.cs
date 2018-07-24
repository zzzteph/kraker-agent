using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Lib.AgentSettings;
using Cracker.Lib.HashCat;
using Cracker.Lib.HttpClient.Data;
using Cracker.Lib.Logging;

namespace Cracker.Lib
{
	public class Agent
	{
		private readonly FiniteStateMachine @switch;
		private JobHandler jobHandler;
		private Task<ExecutionResult> hashcatTask;
		private CancellationTokenSource cts;

		public Agent()
		{
			@switch = new FiniteStateMachine(WaitJob);
		}

		public void WaitJob()
		{
			@switch.SetStateAction(DoNothing);
			
			var job = ClientProxyProvider.Client.GetJob().Result;
			if (job?.Type == null)
			{
				@switch.SetStateAction(WaitJob);
				return;
			}

			jobHandler = JobHandler.Create(job);
			var preparationResult = jobHandler.Prepare();
			if (preparationResult.IsReadyForExecution)
			{
				cts = new CancellationTokenSource();
				var hashcatPath = SettingsProvider.CurrentSettings.Config.HashCatPath;
				Environment.CurrentDirectory = Path.GetDirectoryName(hashcatPath);
				hashcatTask = new HashCatCommandExecuter(preparationResult.HashCatArguments, hashcatPath)
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
				var heartbeat = ClientProxyProvider.Client.Heartbeat().Result;
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