using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kracker.Base;
using Kracker.Base.Domain.Configuration;
using Kracker.Base.Domain.Inventory;
using Kracker.Base.Domain.Jobs;
using Kracker.Base.Injection;
using Kracker.Base.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Kracker.App
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

            Console.CancelKeyPress += (s, o) 
                => cancelKeyPressWater.SetResult(true);

            AddYourselfToWerExcluded(logger);

            InitializeCheckInventoryTimer(logger, config, container.GetService<IInventoryManager>());

            Work(logger, config, container);

            CleanUp();

            RemoveYourselfFromWerExcluded();
        }

        private static void InitializeCheckInventoryTimer(ILogger logger,
            Config config,
            IInventoryManager inventoryManager)
        {
            var inventoryCheckPeriod = TimeSpan.FromSeconds(config.InventoryCheckPeriod.Value);

            checkInventoryTimer = new Timer(async o =>
            {
                try
                {
                    await inventoryManager.UpdateFileDescriptions();
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

        private static void Work(ILogger logger, Config config, IServiceProvider container)
        {
            var hearbeatPeriod = TimeSpan.FromSeconds(config.HearbeatPeriod.Value);
            var agent = container.GetService<IAgent>();
            agentTimer = new Timer(o =>
                {
                    try
                    {
                       var t = agent.Work();
                       t.Wait();
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