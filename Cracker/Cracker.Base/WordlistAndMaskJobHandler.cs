using System;
using System.IO;
using System.Linq;
using Cracker.Base.AgentSettings;
using Cracker.Base.HashCat;
using Cracker.Base.HttpClient.Data;

namespace Cracker.Base
{
	public class WordlistAndMaskJobHandler : JobHandler
	{
		private readonly Job job;
		private TempFilePaths paths;
		public WordlistAndMaskJobHandler(Job job)
		{
			this.job = job;
		}

		public override PrepareJobResult Prepare()
		{
			if (job.JobId == null)
				return new PrepareJobResult
				{
					Error = $"JobId полученной задачи с типом {job.Type} = null, такое мы не выполняем",
					IsReadyForExecution = false
				};

			paths = SettingsProvider.CurrentSettings.TempDirectoryPath.BuildTempFilePaths();
			var hashfile = ClientProxyProvider.Client.GetHashFile(job.HashId).Result;
			if (string.IsNullOrEmpty(hashfile?.Data))
			{
				return new PrepareJobResult
				{
					Error = $"Пустой hashfile: HashId ={job.HashId}, JobId={job.JobId}",
					IsReadyForExecution = false
				};
			}

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


			File.WriteAllBytes(paths.HashFile, Convert.FromBase64String(hashfile.Data));

			var potFile = ClientProxyProvider.Client.GetPotFile(job.HashId).Result;

			File.WriteAllBytes(paths.PotFile, Convert.FromBase64String(potFile?.Data??string.Empty));

			ClientProxyProvider.Client.SendJobStart(job.JobId).ConfigureAwait(false);

			return new PrepareJobResult
			{
				IsReadyForExecution = true,
				HashCatArguments = arguments
			};
		}

		public override void Clear(ExecutionResult executionResult)
		{
			paths.HashFile.SoftDelete("hashfile");

			if (!executionResult.IsSuccessful)
			{
				paths.OutputFile.SoftDelete("outfile");
				paths.PotFile.SoftDelete("potfile");
				return;
			}

			var err = executionResult.Errors.FirstOrDefault(e => e.Contains("No hashes loaded") || e.Contains("Unhandled Exception"));
			if (err != null)
			{
				ClientProxyProvider.Client.SendJobEnd(new { error = err }, job.JobId).ConfigureAwait(false);
				paths.OutputFile.SoftDelete("outfile");
				paths.PotFile.SoftDelete("potfile");
				return;
			}

			var speed = SpeedCalculator.CalculateFact(executionResult.Output);
			if (File.Exists(paths.OutputFile))
			{
				var outfile = Convert.ToBase64String(File.ReadAllBytes(paths.OutputFile));
				var potfile = Convert.ToBase64String(File.ReadAllBytes(paths.PotFile));

				ClientProxyProvider.Client.SendJobEnd(new {outfile, potfile, speed}, job.JobId).ConfigureAwait(false);
				paths.OutputFile.SoftDelete("outfile");
				paths.PotFile.SoftDelete("potfile");
			}
			else
			{
				Logging.Log.Message("Output file не существует, сорян.");
				ClientProxyProvider.Client.SendJobEnd(new {potfile = string.Empty, speed }, job.JobId).ConfigureAwait(false);
			}
		}
	}
}