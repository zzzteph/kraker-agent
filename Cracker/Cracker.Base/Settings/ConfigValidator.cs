using System.IO;
using Cracker.Base.Domain.HashCat;
using Cracker.Base.Model;
using Serilog;

namespace Cracker.Base.Settings
{
    public interface IConfigValidator
    {
        OperationResult Validate();
    }

    public class ConfigValidator : IConfigValidator
    {
        private readonly Config _config;
        private readonly ILogger _logger;
        private readonly IWorkingDirectoryProvider _workingDirectoryProvider;

        public ConfigValidator(Config config,
            ILogger logger, 
            IWorkingDirectoryProvider workingDirectoryProvider)
        {
            _config = config;
            _logger = logger;
            _workingDirectoryProvider = workingDirectoryProvider;
        }

        public OperationResult Validate()
        {
            var allGood = true;

            if (string.IsNullOrEmpty(_config.HashCat.Path))
            {
                _logger.Error($"Doesn't exist the section {nameof(_config.HashCat.Path)} in the config");
                allGood = false;
            }

            var hashcatPath = _workingDirectoryProvider.GetHashCatPath();
            if (!File.Exists(hashcatPath))
            {
                _logger.Error($"Naven't found Hashcat by the path {hashcatPath}");
                allGood = false;
            }

            if (string.IsNullOrEmpty(_config.ServerUrl))
            {
                _logger.Error($"Config doesn't contain the section {nameof(_config.ServerUrl)}");
                allGood = false;
            }

            if (!_config.InventoryCheckPeriod.HasValue)
            {
                _logger.Error($"config doesn't contain the section {nameof(_config.InventoryCheckPeriod)}");
                allGood = false;
            }

            if (!_config.HearbeatPeriod.HasValue)
            {
                _logger.Error($"Config doesn't contain the section {nameof(_config.HearbeatPeriod)}");
                allGood = false;
            }

            return allGood ? OperationResult.Success : OperationResult.Fail("Config isn't correct");
        }
    }
}