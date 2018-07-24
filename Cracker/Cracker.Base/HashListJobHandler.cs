using System;
using System.IO;
using System.Linq;
using Cracker.Lib.AgentSettings;
using Cracker.Lib.HashCat;
using Cracker.Lib.HttpClient.Data;

namespace Cracker.Lib
{
	public class HashListJobHandler:JobHandler
	{
		private TempFilePaths paths;
		private readonly Job job;
		public HashListJobHandler(Job job)
		{
			this.job = job;
		}
		public override PrepareJobResult Prepare()
		{
			if (job.HashId == null)
				return new PrepareJobResult
				{
					IsReadyForExecution = false,
					Error = $"Нет hashId у работы с типом {job.Type}"
				};
			
			var hashfile = ClientProxyProvider.Client.GetHashFile(job.HashId).Result;
			if (string.IsNullOrEmpty(hashfile?.Data))
			{
				return new PrepareJobResult
				{
					Error = $"Пустой hashfile: HashId ={job.HashId}, JobId={job.JobId}",
					IsReadyForExecution = false
				};
			}

			paths = SettingsProvider.CurrentSettings.TempDirectoryPath.BuildTempFilePaths();
			File.WriteAllBytes(paths.HashFile, Convert.FromBase64String(hashfile.Data));
			File.WriteAllText(paths.PotFile, string.Empty);

			string arguments;
			try
			{
				arguments = new ArgumentsBuilder().BuildArguments(job, paths);
			}
			catch (Exception e)
			{
				return new PrepareJobResult
				{
					Error = $"Не смогли сгенерить аргументы для задачи JobId:{job.JobId}, Error: {e}",
					IsReadyForExecution = false
				};
			}

			return new PrepareJobResult
			{
				IsReadyForExecution = true,
				HashCatArguments = arguments
			};
		}

		public override void Clear(ExecutionResult executionResult)
		{
			var error = executionResult.Errors.FirstOrDefault(e => e.Contains("No hashes loaded") || e.Contains("Unhandled Exception"));

			if (paths != null)
			{
				paths.HashFile.SoftDelete("hashfile");
				paths.PotFile.SoftDelete("potfile");
			}

			if (error!=null)
				if (error.Contains("No hashes loaded"))
					ClientProxyProvider.Client.PostAsync<object>(
						$"api/hash/update/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/{job.HashId}",
						() => new { keyspace = 0 }).ConfigureAwait(false);
				else
					ClientProxyProvider.Client.PostAsync<object>(
						$"api/hash/update/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/{job.HashId}",
						() => new { error }).ConfigureAwait(false);
			else
				ClientProxyProvider.Client.PostAsync<object>(
					$"api/hash/update/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/{job.HashId}",
						() => new { keyspace=executionResult.Output.Count }).ConfigureAwait(false);
		}
	}
}