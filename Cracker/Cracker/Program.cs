using System;
using System.Threading;
using Cracker.Base;
using Cracker.Base.AgentSettings;
using Cracker.Base.Logging;

namespace Cracker
{
	partial class Program
	{
		static void Main(string[] args)
		{
			if (!SettingsProvider.CanWorking)
				return;

			var preparedTask = ClientProxyProvider.Client.SendRegistrationKey()
					.ContinueWith(o => ClientProxyProvider.Client.SendAgentInfo())
					.ContinueWith(o => ClientProxyProvider.Client.SendAgentInventory());

			preparedTask.Result.Wait();

			var zero = TimeSpan.FromSeconds(0);
			var inventoryCheckPeriod = TimeSpan.FromSeconds(SettingsProvider.CurrentSettings.Config.InventoryCheckPeriod.Value);

			using (var checkInventoryTimer = new Timer(o =>
			{
				if (AgentInventoryProvider.UpdateFileDescriptions())
					ClientProxyProvider.Client.SendAgentInventory().ConfigureAwait(false);
			}, null, zero, inventoryCheckPeriod))
			{
				var hearbeatPeriod = TimeSpan.FromSeconds(SettingsProvider.CurrentSettings.Config.HearbeatPeriod.Value);
				var agent = new Agent();
				using (var agentTimer = new Timer(o => agent.Work(), null, zero, hearbeatPeriod))
				{
					Log.Message("Агент работает");
					Console.ReadLine();
				}
			}
		}
	}
}
