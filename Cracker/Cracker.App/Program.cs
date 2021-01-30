using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base;
using Cracker.Base.HttpClient;
using Cracker.Base.Logging;
using Cracker.Base.Settings;

namespace Cracker.App
{
    internal class Program
    {
        private static readonly TaskCompletionSource<bool> cancelKeyPressWater = new();

        private static Timer checkInventoryTimer;
        private static Timer agentTimer;

        private static void Main(string[] args)
        {
            var startResult = new Startup().Start();
            if (!startResult.IsSuccess)
            {
                Log.Error($"При старте возникли несовместимые с работой проблемы: {startResult.Error}");
                return;
            }

            var settings = startResult.Result;
            var serverClient = new ServerClient(settings.Config);

            //todo: нужно ли теперь это? 
            Environment.CurrentDirectory = Path.GetDirectoryName(settings.Config.HashCat.Path);
            Console.CancelKeyPress += (s, o) => cancelKeyPressWater.SetResult(true);

            AddYourselfToWerExcluded();

            InitializeAgentForServer(settings, serverClient);

            InitializeCheckInventoryTimer(settings, serverClient);

            Work(settings);

            CleanUp();

            RemoveYourselfFromWerExcluded();
        }

        private static void InitializeAgentForServer(Settings settings, ServerClient serverClient)
        {
            var agentInventoryManager = new AgentInventoryManager(settings.WorkedDirectories);
            serverClient.SendRegistrationKey()
                .ContinueWith(o => serverClient.SendAgentInfo(settings.AgentInfo))
                .ContinueWith(o => serverClient.SendAgentInventory(agentInventoryManager.Get()))
                .Wait();
        }

        private static void InitializeCheckInventoryTimer(Settings settings, ServerClient serverClient)
        {
            var agentInventoryManager = new AgentInventoryManager(settings.WorkedDirectories);
            var inventoryCheckPeriod = TimeSpan.FromSeconds(settings.Config.InventoryCheckPeriod.Value);

            checkInventoryTimer = new Timer(o =>
            {
                try
                {
                    if (agentInventoryManager.UpdateFileDescriptions())
                        serverClient.SendAgentInventory(agentInventoryManager.Get()).ConfigureAwait(false);
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

        private static void Work(Settings settings)
        {
            var hearbeatPeriod = TimeSpan.FromSeconds(settings.Config.HearbeatPeriod.Value);
            var agent = new Agent(settings);
            agentTimer = new Timer(o =>
                {
                    try
                    {
                        agent.Work();
                    }
                    catch (Exception e)
                    {
                        Log.Message($"Словили необработанное исключение в работе агента: {e}");
                        agent = new Agent(settings);
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