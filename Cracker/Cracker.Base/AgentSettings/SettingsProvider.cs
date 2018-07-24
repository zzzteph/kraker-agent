using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Cracker.Lib.AgentSettings
{
	public static class SettingsProvider
	{
		private static readonly Settings settings;

		static SettingsProvider()
		{
			CanWorking = true;
			var currentDirectory = Directory.GetCurrentDirectory();

			settings = new Settings
			{
				WordlistPath = Path.Combine(currentDirectory, "wordlist"),
				RulesPath = Path.Combine(currentDirectory, "rules"),
				TempDirectoryPath = Path.Combine(currentDirectory, "tmp")

			};
			
			var configPatn = Path.Combine(currentDirectory, "appsettings.json");
			if (!File.Exists(configPatn))
			{
				Logging.Log.Message($"Не существует конфигурационный файл {configPatn}");
				CanWorking = false;
			}
			else
			{
				settings.Config = JObject.Parse(File.ReadAllText(configPatn)).ToObject<Config>();

				if (string.IsNullOrEmpty(settings.Config.HashCatPath))
				{
					Logging.Log.Message($"{configPatn} не содержит секцию {nameof(settings.Config.HashCatPath)}");
					CanWorking = false;
				}

				if (!File.Exists(settings.Config.HashCatPath))
				{
					Logging.Log.Message($"Hashcat не найден по пути {settings.Config.HashCatPath}");
					CanWorking = false;
				}

				if (string.IsNullOrEmpty(settings.Config.ServerUrl))
				{
					Logging.Log.Message($"{configPatn} не содержит секцию {nameof(settings.Config.ServerUrl)}");
					CanWorking = false;
				}
				
				if (!settings.Config.InventoryCheckPeriod.HasValue)
				{
					Logging.Log.Message($"{configPatn} не содержит секцию {nameof(settings.Config.InventoryCheckPeriod)}");
					CanWorking = false;
				}

				if (!settings.Config.HearbeatPeriod.HasValue)
				{
					Logging.Log.Message($"{configPatn} не содержит секцию {nameof(settings.Config.HearbeatPeriod)}");
					CanWorking = false;
				}
			}

			if (!CanWorking)
				return;

			var agentInfo = AgentInfoProvider.Get();
			var configAI = settings.Config.AgentInfo;

			if (configAI == null
				|| configAI.OperationalSystem != agentInfo.OperationalSystem
				|| configAI.HostName != agentInfo.HostName
				|| configAI.Ip != agentInfo.Ip
				|| string.IsNullOrEmpty(settings.Config.RegistrationKey))
			{
				settings.Config.RegistrationKey = Guid.NewGuid().ToString();
			}

			settings.Config.AgentInfo = agentInfo;
			
			if (!Directory.Exists(settings.WordlistPath))
				Directory.CreateDirectory(settings.WordlistPath);

			if (!Directory.Exists(settings.RulesPath))
				Directory.CreateDirectory(settings.RulesPath);

			Directory.CreateDirectory(settings.TempDirectoryPath);

			File.WriteAllText(configPatn, JObject.FromObject(settings.Config).ToString());
		}

		public static Settings CurrentSettings => settings;
		public static bool CanWorking { get; }
	}
}