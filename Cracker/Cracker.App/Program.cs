using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base;
using Cracker.Base.Domain.Inventory;
using Cracker.Base.Injection;
using Cracker.Base.Model;
using Cracker.Base.Services;
using Cracker.Base.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Cracker.App
{
    internal class Program
    {
        private static readonly TaskCompletionSource<bool> cancelKeyPressWater = new();

        private static Timer checkInventoryTimer;
        private static Timer agentTimer;

        private static async Task Main(string[] args)
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var config = configurationRoot.Get<Config>();
            
            var container = ServiceProviderBuilder.Build(config);
            var logger = container.GetService<ILogger>();

            var startResult = await container.GetService<IStartup>().Start();
            if (!startResult.IsSuccess)
            {
                logger.Error($"Can't work. Reason: {startResult.Error}");
                return;
            }

            var (agentId, agentInfo, inventory) = startResult.Result;
            var krakerApi = container.GetService<IKrakerApi>();

            Console.CancelKeyPress += (s, o) => cancelKeyPressWater.SetResult(true);

            AddYourselfToWerExcluded(logger);

            await InitializeAgentForServer(agentId, agentInfo, inventory, krakerApi);

            InitializeCheckInventoryTimer(logger, agentId, config, krakerApi, container.GetService<IInventoryManager>());

            Work(logger, config, container);

            CleanUp();

            RemoveYourselfFromWerExcluded();
        }

        private static async Task InitializeAgentForServer(AgentId agentId, AgentInfo agentInfo, Inventory inventory, IKrakerApi krakerApi)
        {
            await krakerApi.SendAgentInfo(agentId.Id, agentInfo);
            await krakerApi.SendAgentInventory(agentId.Id, inventory.Files);
        }

        private static void InitializeCheckInventoryTimer(ILogger logger,
            AgentId agentId,
            Config config,
            IKrakerApi krakerApi,
            IInventoryManager inventoryManager)
        {
            var inventoryCheckPeriod = TimeSpan.FromSeconds(config.InventoryCheckPeriod.Value);

            checkInventoryTimer = new Timer(o =>
            {
                try
                {
                    var (inventoryWasChanged, inventory) = inventoryManager.UpdateFileDescriptions();
                    if (inventoryWasChanged)
                        krakerApi.SendAgentInventory(agentId.Id, inventory.Files);
                }
                catch (Exception e)
                {
                    logger.Error($"Словили исключение при проверке инвентаря: {e}");
                }
                finally
                {
                    checkInventoryTimer.Change(inventoryCheckPeriod, TimeSpan.FromMilliseconds(-1));
                }
            }, null, TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(-1));
        }

        private static void Work(ILogger logger,Config config, IServiceProvider container)
        {
            var hearbeatPeriod = TimeSpan.FromSeconds(config.HearbeatPeriod.Value);
            var agent = container.GetService<IAgent>();
            agentTimer = new Timer(o =>
                {
                    try
                    {
                        agent.Work();
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Got an unhandled exception: {e}");
                        agent = container.GetService<IAgent>();
                    }
                },
                null, TimeSpan.FromSeconds(0), hearbeatPeriod);

            logger.Information("An agent is working now");
            cancelKeyPressWater.Task.Wait();
            logger.Information("An agent's stopped");
        }

        private static void CleanUp()
        {
            checkInventoryTimer.Dispose();
            agentTimer.Dispose();
        }

        private static void AddYourselfToWerExcluded(ILogger logger)
        {
            var pwzExeName = Process.GetCurrentProcess().MainModule.FileName;
            var res = Wer.WerAddExcludedApplication(pwzExeName, false);
            if (res != 0)
                logger.Information("Can't turn off WER for the process. Try to run the application under an admin role");

            Wer.SetErrorMode(ErrorModes.SEM_NONE);
        }

        private static void RemoveYourselfFromWerExcluded()
        {
            var pwzExeName = Process.GetCurrentProcess().MainModule.FileName;
            Wer.WerRemoveExcludedApplication(pwzExeName, false);
        }
    }
}