using System.IO;
using Cracker.Base.Model;
using Serilog;
using static Cracker.Base.Model.Constants;

namespace Cracker.Base.Settings
{
    public interface IWorkedFoldersManager
    {
        WorkedFolders Prepare();
    }

    public class WorkedFoldersManager : IWorkedFoldersManager
    {
        private readonly ILogger _logger;
        private readonly string _appDirectory;

        public WorkedFoldersManager(ILogger logger, AppFolder appFolder)
        {
            _appDirectory = appFolder.Value;
            _logger = logger;
        }

        public WorkedFolders Prepare()
        {
            var workedDirectories = new WorkedFolders
            {
                WordlistPath = Path.Combine(_appDirectory, WordlistsFolder),
                RulesPath = Path.Combine(_appDirectory, RulesFolder),
                TempFolderPath = Path.Combine(_appDirectory,ArtefactsFolder, TempFolder)
            };

            if (!Directory.Exists(workedDirectories.WordlistPath))
                Directory.CreateDirectory(workedDirectories.WordlistPath);

            if (!Directory.Exists(workedDirectories.RulesPath))
                Directory.CreateDirectory(workedDirectories.RulesPath);

            Directory.CreateDirectory(workedDirectories.TempFolderPath);
            return workedDirectories;
        }
    }
}