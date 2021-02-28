using System;
using System.Threading.Tasks;
using Kracker.Base.Domain;
using Kracker.Base.Domain.Configuration;
using Kracker.Base.Domain.HashCat;
using Kracker.Base.Domain.Inventory;
using Kracker.Base.Services;
using Kracker.Base.Tools;

namespace Kracker.Base
{
    public interface IStartup
    {
        Task<OperationResult> Start();
    }

    public class Startup : IStartup
    {
        private readonly IConfigValidator _configValidator;
        private readonly IAgentRegistrationManager _registrationManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IWorkingDirectoryProvider _workingDirectoryProvider;
        
        private readonly IKrakerApi _krakerApi;
        private readonly Config _config;

        public Startup(IKrakerApi krakerApi, 
            IConfigValidator configValidator,
            Config config,
            IAgentRegistrationManager registrationManager,
            IInventoryManager inventoryManager,
            IWorkingDirectoryProvider workingDirectoryProvider)
        {
            _krakerApi = krakerApi;
            _configValidator = configValidator;
            _config = config;
            _registrationManager = registrationManager;
            _inventoryManager = inventoryManager;
            _workingDirectoryProvider = workingDirectoryProvider;
        }

        public async Task<OperationResult> Start()
        {
            Environment.CurrentDirectory = _workingDirectoryProvider.Get();
            
            var configResult = _configValidator.Validate();
            if (!configResult.IsSuccess)
                return OperationResult.Fail(configResult.Error ?? "The config is incorrect");

            await _registrationManager.Register();
            
            var inventoryResult = await _inventoryManager.Initialize();

            if (!inventoryResult.IsSuccess)
                return OperationResult.Fail(inventoryResult.Error?? "Inventory isn't initialized");

            return OperationResult.Success;
        }
    }
}