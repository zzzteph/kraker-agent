using System;
using System.IO;
using Cracker.Base.Model;

namespace Cracker.Base.Settings
{
	public class Startup
	{
		private readonly ConfigManager configManager;
		private readonly AgentInfoManager agentInfoManager;
		private readonly WorkedDirectoriesManager workedDirectoriesManager;

		public Startup()
		{
			var currentDirectory = Directory.GetCurrentDirectory();
			configManager = new ConfigManager(currentDirectory);
			agentInfoManager = new AgentInfoManager(currentDirectory);
			workedDirectoriesManager = new WorkedDirectoriesManager(currentDirectory);
		}

		public OperationResult<Settings> Start()
		{

			var configResult = configManager.Build();
			if (!configResult.IsSuccess)
				return OperationResult<Settings>.Fail(configResult.Error);

			var config = configResult.Result;

			var agentInfo = agentInfoManager.Build(config);
			var oldAgentInfoResult = agentInfoManager.GetFromFile();
			if (!oldAgentInfoResult.IsSuccess)
				return OperationResult<Settings>.Fail(oldAgentInfoResult.Error);

			var oldAgentInfo = oldAgentInfoResult.Result;

			if (NeedGenerateNewRegistrationKey(config, agentInfo, oldAgentInfo))
			{
				config.RegistrationKey = Guid.NewGuid().ToString();

				var saveConfig = configManager.Save(config);
				if (!saveConfig.IsSuccess)
					return OperationResult<Settings>.Fail(saveConfig.Error);

				var saveAgentInfo = agentInfoManager.Save(agentInfo);
				if (!saveAgentInfo.IsSuccess)
					return OperationResult<Settings>.Fail(saveAgentInfo.Error);
			}

			var workedDirectoriesResult = workedDirectoriesManager.Prepare();
			if (! workedDirectoriesResult.IsSuccess)
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

		private bool NeedGenerateNewRegistrationKey(Config config, AgentInfo actualInfo, AgentInfo oldInfo) =>
			oldInfo == null
			|| oldInfo.OperationalSystem != actualInfo.OperationalSystem
			|| oldInfo.HostName != actualInfo.HostName
			|| oldInfo.Ip != actualInfo.Ip
			|| string.IsNullOrEmpty(config.RegistrationKey);
	}
}