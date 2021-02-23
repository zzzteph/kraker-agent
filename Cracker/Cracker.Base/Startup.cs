﻿using System;
using System.IO;
using System.Threading.Tasks;
using Cracker.Base.Domain.Inventory;
using Cracker.Base.Model;
using Cracker.Base.Services;
using Cracker.Base.Settings;

namespace Cracker.Base
{
    public interface IStartup
    {
        Task<OperationResult<(AgentId agentId, AgentInfo agentInfo, Inventory inventory)>> Start();
    }

    public class Startup : IStartup
    {
        private readonly IConfigValidator _configValidator;
        private readonly IAgentRegistrationManager _registrationManager;
        private readonly IInventoryManager inventoryManager;
        
        private readonly IKrakerApi _krakerApi;
        private readonly Config _config;

        public Startup(IKrakerApi krakerApi, 
            IConfigValidator configValidator,
            Config config,
            IAgentRegistrationManager registrationManager,
            IInventoryManager inventoryManager)
        {
            _krakerApi = krakerApi;
            _configValidator = configValidator;
            _config = config;
            _registrationManager = registrationManager;
            this.inventoryManager = inventoryManager;
        }

        public async Task<OperationResult<(AgentId agentId, AgentInfo agentInfo, Inventory inventory)>> Start()
        {
            var configResult = _configValidator.Validate();
            if (!configResult.IsSuccess)
                return OperationResult<(AgentId, AgentInfo, Inventory)>.Fail(configResult.Error);

            var (agentId, agentInfo) = await _registrationManager.Register();
            
            Environment.CurrentDirectory = Path.GetDirectoryName(_config.HashCat.Path);


            var inventoryResult = inventoryManager.Initialize();

            if (!inventoryResult.IsSuccess)
                return OperationResult<(AgentId, AgentInfo, Inventory)>.Fail(inventoryResult.Error);

            return OperationResult<(AgentId, AgentInfo, Inventory)>
                .Success((agentId, agentInfo, inventoryResult.Result));
        }
    }
}