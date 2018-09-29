using System;
using System.Linq;
using Cracker.Base.HashCat;
using Cracker.Base.HttpClient.Data;

namespace Cracker.Base
{
	public class TemplateJobHandler : JobHandler
	{
		private readonly Job job;
		public TemplateJobHandler(Job job, Settings.Settings settings):base(settings)
		{
			this.job = job;
		}
		public override PrepareJobResult Prepare()
		{
			try
			{
				var arguments = new ArgumentsBuilder(settings.WorkedDirectories).BuildArguments(job, null);
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
			serverClient.Client.PostAsync<object>(
				$"api/template/update/{settings.Config.RegistrationKey}/{job.TemplateId}",
				() => new {keyspace}).ConfigureAwait(false);
		}
	}
}