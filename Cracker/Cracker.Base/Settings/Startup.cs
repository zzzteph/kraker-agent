using System;
using System.IO;
using Cracker.Base.Model;
using Cracker.Base.Services;
using Refit;

namespace Cracker.Base.Settings
{
    public interface IStartup
    {
        OperationResult<Settings> Start();
        bool NeedGenerateNewRegistrationKey(Config config, AgentInfo actualInfo, AgentInfo oldInfo);
    }

    public class Startup : IStartup
    {
        private readonly AgentInfoManager _agentInfoManager;
        private readonly ConfigManager _configManager;
        private readonly WorkedDirectoriesManager _workedDirectoriesManager;
        private readonly IKrakerApi _krakerApi;

        public Startup(IKrakerApi krakerApi, 
            AgentInfoManager agentInfoManager,
            ConfigManager configManager,
            WorkedDirectoriesManager workedDirectoriesManager)
        {
            _krakerApi = krakerApi;
            _configManager = configManager;
            _agentInfoManager = agentInfoManager;
            _workedDirectoriesManager = workedDirectoriesManager;
        }

        public OperationResult<Settings> Start()
        {
            var configResult = _configManager.Build();
            if (!configResult.IsSuccess)
                return OperationResult<Settings>.Fail(configResult.Error);

            var config = configResult.Result;

            var agentInfo = _agentInfoManager.Build(config.HashCat, RestService.For<IKrakerApi>(""));
            var oldAgentInfoResult = _agentInfoManager.GetFromFile();
            if (!oldAgentInfoResult.IsSuccess)
                return OperationResult<Settings>.Fail(oldAgentInfoResult.Error);

            var oldAgentInfo = oldAgentInfoResult.Result;

            if (NeedGenerateNewRegistrationKey(config, agentInfo, oldAgentInfo))
            {
                config.AgentId = Guid.NewGuid().ToString();

                var saveConfig = _configManager.Save(config);
                if (!saveConfig.IsSuccess)
                    return OperationResult<Settings>.Fail(saveConfig.Error);

                var saveAgentInfo = _agentInfoManager.Save(agentInfo);
                if (!saveAgentInfo.IsSuccess)
                    return OperationResult<Settings>.Fail(saveAgentInfo.Error);
            }

            var workedDirectoriesResult = _workedDirectoriesManager.Prepare();
            if (!workedDirectoriesResult.IsSuccess)
                return OperationResult<Settings>.Fail(workedDirectoriesResult.Error);

            var agentInventoryInitializeResult =
                new AgentInventoryManager(workedDirectoriesResult.Result)
                    .Initialize();

            if (!agentInventoryInitializeResult.IsSuccess)
                return OperationResult<Settings>.Fail(agentInventoryInitializeResult.Error);

            return OperationResult<Settings>.Success(new Settings
            {
                AgentInfo = agentInfo,
                Config = config,
                WorkedDirectories = workedDirectoriesResult.Result
            });
        }

        public bool NeedGenerateNewRegistrationKey(Config config, AgentInfo actualInfo, AgentInfo oldInfo)
        {
            return oldInfo == null
                   || oldInfo.OperationalSystem != actualInfo.OperationalSystem
                   || oldInfo.HostName != actualInfo.HostName
                   || oldInfo.Ip != actualInfo.Ip
                   || string.IsNullOrEmpty(config.AgentId);
        }
    }
}