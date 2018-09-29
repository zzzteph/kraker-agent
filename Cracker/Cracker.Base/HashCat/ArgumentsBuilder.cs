using System;
using System.Collections.Generic;
using System.IO;
using Cracker.Base.HttpClient.Data;
using Cracker.Base.Settings;

namespace Cracker.Base.HashCat
{
    public class ArgumentsBuilder
	{
		private readonly IDictionary<string, Func<Job, TempFilePaths, string>> maps;
		private readonly WorkedDirectories workedDirectories;

		public ArgumentsBuilder(WorkedDirectories workedDirectories)
		{
			maps = new Dictionary<string, Func<Job, TempFilePaths, string>>
			{
				[JobType.Template] = BuildTemplateJobArguments,
				[JobType.HashList] = BuildHashlistJobArguments,
				[JobType.Mask] = BuildMaskJobArguments,
				[JobType.Wordlist] = BuildWordlistJobArguments
			};
			this.workedDirectories = workedDirectories;
		}
		public string BuildArguments(Job job, TempFilePaths paths) => maps[job.Type](job, paths);
		private string BuildTemplateJobArguments(Job job, TempFilePaths paths)
		{
			string command;
			switch (job.TemplateType)
			{
				case TemplateType.Mask:
					command = job.Command.TemplateOptions.Mask.ToString();
					break;
				case TemplateType.Wordlist:
					command = BuildR(job.Command.TemplateOptions.Wordlist)
							+ $" \"{Path.Combine(workedDirectories.WordlistPath, job.Command.TemplateOptions.Wordlist.Wordlist)}\"";
					break;
				default:
					throw new ArgumentException($"Кривой template_type для задачи, с таким не работаем: {job.TemplateType}");
			}

			return $"{BuildOptions(job.Command)} {command}";
		}

		private string BuildHashlistJobArguments(Job job, TempFilePaths paths)
		{
			var c = job.Command;
			return $"{BuildOptions(c)} -m {c.M} {BuildFilePaths(paths)}";
		}

		private string BuildMaskJobArguments(Job job, TempFilePaths paths)
		{
			var filePaths = BuildFilePaths(paths);
			var c = job.Command;
			return $"{BuildOptions(c)} --skip={c.Skip} --limit={c.Limit} -m {c.M} "
					+ $" --outfile=\"{paths.OutputFile}\" " 
					+ $"{filePaths} {c.TemplateOptions.Mask}";
		}

		private string BuildWordlistJobArguments(Job job, TempFilePaths paths)
		{
			var c = job.Command;
			var filePaths = BuildFilePaths(paths);;

			return $"{BuildOptions(c)} --skip={c.Skip} --limit={c.Limit} -m {c.M} "
					+ BuildR(c.TemplateOptions.Wordlist)
						+ $" --outfile=\"{paths.OutputFile}\" " +
						$"{filePaths} \"{Path.Combine(workedDirectories.WordlistPath, c.TemplateOptions.Wordlist.Wordlist)}\""; 
		}

		private string BuildR(WordlistTemplate wt) => string.IsNullOrEmpty(wt?.Rule)
				? string.Empty
				: $"-r \"{Path.Combine(workedDirectories.RulesPath, wt.Rule)}\"";

		private string BuildOptions(Command command) => string.Join(" ", command.Options);

		public string BuildFilePaths(TempFilePaths paths) => paths.PotFile == null
				? $" \"{paths.HashFile}\" "
				: $"--potfile-path=\"{paths.PotFile}\" \"{paths.HashFile}\" ";
	}
}
