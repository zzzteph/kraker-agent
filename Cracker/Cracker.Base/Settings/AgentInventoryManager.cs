using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cracker.Base.Logging;
using Cracker.Base.Model;
using Newtonsoft.Json.Linq;

namespace Cracker.Base.Settings
{
    public class AgentInventoryManager
    {
        private readonly FileDescriptionBuilder descriptionBuilder;
        private readonly string inventoryFilePath;
        private readonly WorkedDirectories workedDirectories;
        private IDictionary<string, FileDescription> fileDescriptions;

        public AgentInventoryManager(WorkedDirectories workedDirectories)
        {
            this.workedDirectories = workedDirectories;
            inventoryFilePath = Path.Combine(workedDirectories.TempDirectoryPath, "agent inventory temp file");
            descriptionBuilder = new FileDescriptionBuilder();
        }

        public OperationResult Initialize()
        {
            try
            {
                if (File.Exists(inventoryFilePath))
                {
                    fileDescriptions = JObject.Parse(File.ReadAllText(inventoryFilePath))
                        .ToObject<Dictionary<string, FileDescription>>();
                }
                else
                {
                    fileDescriptions = Directory.GetFiles(workedDirectories.RulesPath)
                        .Concat(Directory.GetFiles(workedDirectories.WordlistPath))
                        .ToDictionary(p => p, descriptionBuilder.Build);

                    File.WriteAllText(inventoryFilePath, JObject.FromObject(fileDescriptions).ToString());
                }

                return OperationResult.Success;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return OperationResult.Fail("[Инветарь] Не удалось подготовить инвентарь");
            }
        }


        public FileDescription[] Get()
        {
            return fileDescriptions.Values.ToArray();
        }

        public bool UpdateFileDescriptions()
        {
            Log.Message("[Инветарь] Пришла пора проверить инвентарь");

            var isChanged = false;

            var currentFiles = Directory.GetFiles(workedDirectories.RulesPath)
                .Concat(Directory.GetFiles(workedDirectories.WordlistPath))
                .ToList();

            foreach (var currentFile in currentFiles)
            {
                if (fileDescriptions.TryGetValue(currentFile, out var oldFileDescription)
                    && File.GetLastWriteTime(currentFile) == oldFileDescription.LastWriteTime)
                    continue;
                try
                {
                    Log.Message($"[Инветарь] Нашлось что-то новенькое: {currentFile}");
                    fileDescriptions[currentFile] = descriptionBuilder.Build(currentFile);
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
                Log.Message("[Инветарь] Обнаружены изменения, сохраняю новые данные в своих закромах");
                File.WriteAllText(inventoryFilePath, JObject.FromObject(fileDescriptions).ToString());
            }
            else
            {
                Log.Message("[Инветарь] Проверка завершена, изменения не обнаружены");
            }

            return isChanged;
        }
    }
}