using System;
using System.Threading.Tasks;
using Cracker.Base.Domain.HashCat;
using Cracker.Base.Domain.Inventory;
using Cracker.Base.Model;
using Cracker.Base.Services;
using Cracker.Base.Settings;

namespace Cracker.Base
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
                return OperationResult.Fail(configResult.Error);

            await _registrationManager.Register();
            
            var inventoryResult = await _inventoryManager.Initialize();

            if (!inventoryResult.IsSuccess)
                return OperationResult.Fail(inventoryResult.Error);

            return OperationResult.Success;
        }
    }
}