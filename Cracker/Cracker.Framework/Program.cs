using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base;
using Cracker.Base.AgentSettings;
using Cracker.Base.Logging;

namespace Cracker.Framework
{
	class Program
	{
		private static TaskCompletionSource<bool> waiter = new TaskCompletionSource<bool>();
		private static readonly Config config = SettingsProvider.CurrentSettings.Config;

		private static Timer checkInventoryTimer;
		private static Timer agentTimer;

		static void Main(string[] args)
		{
			Environment.CurrentDirectory = Path.GetDirectoryName(SettingsProvider.CurrentSettings.Config.HashCatPath);

			if (!SettingsProvider.CanWorking)
				return;

			Console.CancelKeyPress += (s, o) => waiter.SetResult(true);

			AddYourselfToWerExcluded();

			InitializeAgentForServer();

			InitializeCheckInventoryTimer();

			Work();

			CleanUp();

			RemoveYourselfFromWerExcluded();
		}

		private static void InitializeAgentForServer()
		{
			ClientProxyProvider.Client.SendRegistrationKey()
				.ContinueWith(o => ClientProxyProvider.Client.SendAgentInfo())
				.ContinueWith(o => ClientProxyProvider.Client.SendAgentInventory())
				.Wait();
		}

		private static void InitializeCheckInventoryTimer()
		{
			var inventoryCheckPeriod = TimeSpan.FromSeconds(config.InventoryCheckPeriod.Value);
			checkInventoryTimer = new Timer(o =>
			{
				try
				{
					if (AgentInventoryProvider.UpdateFileDescriptions())
						ClientProxyProvider.Client.SendAgentInventory().ConfigureAwait(false);
				}
				catch (Exception e)
				{
					Log.Message($"Словили исключение при проверке инвентаря: {e.ToString()}");
				}
				finally
				{
					checkInventoryTimer.Change(inventoryCheckPeriod, TimeSpan.FromMilliseconds(-1));
				}
			}, null, TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(-1));
		}

		private static void Work()
		{
			var hearbeatPeriod = TimeSpan.FromSeconds(config.HearbeatPeriod.Value);
			var agent = new Agent();
			agentTimer = new Timer(o =>
				{
					try
					{
						agent.Work();
					}
					catch (Exception e)
					{
						Log.Message($"Словили необработанное исключение в работе агента: {e.ToString()}");
						agent = new Agent();
					}
				},
				null, TimeSpan.FromSeconds(0), hearbeatPeriod);

			Log.Message("Агент работает");
			waiter.Task.Wait();
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