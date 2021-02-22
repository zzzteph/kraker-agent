using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base;
using Cracker.Base.Injection;
using Cracker.Base.Logging;
using Cracker.Base.Services;
using Cracker.Base.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Cracker.App
{
    internal class Program
    {
        private static readonly TaskCompletionSource<bool> cancelKeyPressWater = new();

        private static Timer checkInventoryTimer;
        private static Timer agentTimer;

        private static void Main(string[] args)
        {
            var container = ServiceProviderBuilder.Build();
            var startResult = container.GetService<IStartup>().Start();
            if (!startResult.IsSuccess)
            {
                Log.Error($"При старте возникли несовместимые с работой проблемы: {startResult.Error}");
                return;
            }

            var settings = startResult.Result;
            var krakerApi = container.GetService<IKrakerApi>();

            //todo: нужно ли теперь это? 
            Environment.CurrentDirectory = Path.GetDirectoryName(settings.Config.HashCat.Path);
            Console.CancelKeyPress += (s, o) => cancelKeyPressWater.SetResult(true);

            AddYourselfToWerExcluded();

            InitializeAgentForServer(settings, krakerApi);

            InitializeCheckInventoryTimer(settings, krakerApi);

            Work(settings, container);

            CleanUp();

            RemoveYourselfFromWerExcluded();
        }

        private static void InitializeAgentForServer(Settings settings, IKrakerApi krakerApi)
        {
            var agentInventoryManager = new AgentInventoryManager(settings.WorkedDirectories);
            krakerApi.RegisterAgent()
                .ContinueWith(o => krakerApi.SendAgentInfo(settings.Config.AgentId, settings.AgentInfo))
                .ContinueWith(o => krakerApi.SendAgentInventory(settings.Config.AgentId, agentInventoryManager.Get()))
                .Wait();
        }

        private static void InitializeCheckInventoryTimer(Settings settings, IKrakerApi krakerApi)
        {
            var agentInventoryManager = new AgentInventoryManager(settings.WorkedDirectories);
            var inventoryCheckPeriod = TimeSpan.FromSeconds(settings.Config.InventoryCheckPeriod.Value);

            checkInventoryTimer = new Timer(o =>
            {
                try
                {
                    if (agentInventoryManager.UpdateFileDescriptions())
                        krakerApi.SendAgentInventory(settings.Config.AgentId, agentInventoryManager.Get());
                }
                catch (Exception e)
                {
                    Log.Message($"Словили исключение при проверке инвентаря: {e}");
                }
                finally
                {
                    checkInventoryTimer.Change(inventoryCheckPeriod, TimeSpan.FromMilliseconds(-1));
                }
            }, null, TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(-1));
        }

        private static void Work(Settings settings, IServiceProvider container)
        {
            var hearbeatPeriod = TimeSpan.FromSeconds(settings.Config.HearbeatPeriod.Value);
            var agent = new Agent(settings, container.GetService<IJobHandlerProvider>());
            agentTimer = new Timer(o =>
                {
                    try
                    {
                        agent.Work();
                    }
                    catch (Exception e)
                    {
                        Log.Message($"Словили необработанное исключение в работе агента: {e}");
                        agent = container.GetService<Agent>();
                    }
                },
                null, TimeSpan.FromSeconds(0), hearbeatPeriod);

            Log.Message("Агент работает");
            cancelKeyPressWater.Task.Wait();
            Log.Message("Ой, всё!");
        }

        private static void CleanUp()
        {
            checkInventoryTimer.Dispose();
            agentTimer.Dispose();
        }

        private static void AddYourselfToWerExcluded()
        {
            var pwzExeName = Process.GetCurrentProcess().MainModule.FileName;
            var res = Wer.WerAddExcludedApplication(pwzExeName, false);
            if (res != 0)
                Log.Message("Не удалось отрубить WER для процесса, запускать надо из-под администратора");

            Wer.SetErrorMode(ErrorModes.SEM_NONE);
        }

        private static void RemoveYourselfFromWerExcluded()
        {
            var pwzExeName = Process.GetCurrentProcess().MainModule.FileName;
            Wer.WerRemoveExcludedApplication(pwzExeName, false);
        }
    }
}