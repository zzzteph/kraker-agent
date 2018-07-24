using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cracker.Lib.Logging;
using Newtonsoft.Json.Linq;

namespace Cracker.Lib.AgentSettings
{ 
	public static class AgentInventoryProvider
	{
		private static readonly IDictionary<string, FileDescription> fileDescriptions;
		private static readonly string aiFilePath;


		static AgentInventoryProvider()
		{
			aiFilePath = Path.Combine(SettingsProvider.CurrentSettings.TempDirectoryPath, "agent inventory temp file");

			if(File.Exists(aiFilePath))
				fileDescriptions = JObject.Parse(File.ReadAllText(aiFilePath)).ToObject<Dictionary<string, FileDescription>>();
			else
			{
				fileDescriptions = Directory.GetFiles(SettingsProvider.CurrentSettings.RulesPath)
					.Concat(Directory.GetFiles(SettingsProvider.CurrentSettings.WordlistPath))
					.ToDictionary(p => p, FileDescriptionBuilder.Build);

				File.WriteAllText(aiFilePath, JObject.FromObject(fileDescriptions).ToString());
			}
		}

		public static FileDescription[] Get()
		{
			return fileDescriptions.Values.ToArray();
		}

		public static bool UpdateFileDescriptions()
		{
			Log.Message("[Инветарь] Пришла пора проверить инвентарь");

			var isChanged = false;

			var currentFiles = Directory.GetFiles(SettingsProvider.CurrentSettings.RulesPath)
				.Concat(Directory.GetFiles(SettingsProvider.CurrentSettings.WordlistPath))
				.ToList();

			foreach (var currentFile in currentFiles)
			{
				if (fileDescriptions.TryGetValue(currentFile, out var oldFileDescription)
					&& File.GetLastWriteTime(currentFile) == oldFileDescription.LastWriteTime)
					continue;
				try
				{
					Log.Message($"[Инветарь] Нашлось что-то новенькое: {currentFile}");
					fileDescriptions[currentFile] = FileDescriptionBuilder.Build(currentFile);
					isChanged = true;
				}
				catch (Exception e)
				{
					Log.Message($"[Инветарь] Не смогли пересчитать fileDescription для файла {currentFile}: {e}");
				}
			}

			var deletedFiles = fileDescriptions.Keys.Except(currentFiles).ToList();

			foreach (var deletedFile in deletedFiles)
			{
				Log.Message($"[Инветарь] Обнаружилась потеря бойца {deletedFile}");
				fileDescriptions.Remove(deletedFile);
				isChanged = true;
			}

			if (isChanged)
			{
				Log.Message($"[Инветарь] Обнаружены изменения, сохраняю новые данные в своих закромах");
				File.WriteAllText(aiFilePath, JObject.FromObject(fileDescriptions).ToString());
			}
			else
				Log.Message("[Инветарь] Проверка завершена, изменения не обнаружены");

			return isChanged;
		}
	}
}