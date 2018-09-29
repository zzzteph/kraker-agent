using System;
using System.IO;
using Cracker.Base.Logging;
using Cracker.Base.Model;

namespace Cracker.Base.Settings
{
	public class WorkedDirectoriesManager
	{
		private readonly string currentDirectory;
		public WorkedDirectoriesManager(string currentDirectory)
		{
			this.currentDirectory = currentDirectory;
		}
		public OperationResult<WorkedDirectories> Prepare()
		{
			var workedDirectories = new WorkedDirectories
			{
				WordlistPath = Path.Combine(currentDirectory, "wordlist"),
				RulesPath = Path.Combine(currentDirectory, "rules"),
				TempDirectoryPath = Path.Combine(currentDirectory, "tmp")
			};
			try
			{
				if (!Directory.Exists(workedDirectories.WordlistPath))
					Directory.CreateDirectory(workedDirectories.WordlistPath);

				if (!Directory.Exists(workedDirectories.RulesPath))
					Directory.CreateDirectory(workedDirectories.RulesPath);

				Directory.CreateDirectory(workedDirectories.TempDirectoryPath);
				return OperationResult<WorkedDirectories>.Success(workedDirectories);
			}
			catch (Exception e)
			{
				Log.Error(e);
				return OperationResult<WorkedDirectories>.Fail("Не удалось подготовить дирректории для работы агента (wordlist, rules, tmp)");
			}
		}
	}
}