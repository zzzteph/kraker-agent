using System;
using System.Linq;
using Cracker.Lib.AgentSettings;
using Cracker.Lib.HashCat;
using Cracker.Lib.HttpClient.Data;

namespace Cracker.Lib
{
	public class TemplateJobHandler : JobHandler
	{
		private readonly Job job;
		public TemplateJobHandler(Job job)
		{
			this.job = job;
		}
		public override PrepareJobResult Prepare()
		{
			try
			{
				var arguments = new ArgumentsBuilder().BuildArguments(job, null);
				return new PrepareJobResult
				{
					HashCatArguments = arguments,
					IsReadyForExecution = true
				};
			}
			catch (Exception e)
			{
				return new PrepareJobResult
				{
					Error = $"Не смогли сгенерить аргументы для задачи JobId:{job.JobId}, Error: {e}",
					IsReadyForExecution = false
				};
			}
		}

		public override void Clear(ExecutionResult executionResult)
		{
			var keyspace = executionResult?.Output.LastOrDefault(o => o != null);
			ClientProxyProvider.Client.PostAsync<object>(
				$"api/template/update/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/{job.TemplateId}",
				() => new {keyspace}).ConfigureAwait(false);
		}
	}
}