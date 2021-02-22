using System;
using System.IO;
using System.Text.Json;
using Cracker.Base.Logging;
using Cracker.Base.Model;

namespace Cracker.Base.Settings
{
    public class ConfigManager
    {
        private readonly string configPatn;

        public ConfigManager(string currentDirectory)
        {
            configPatn = Path.Combine(currentDirectory, "appsettings.json");
        }

        public OperationResult<Config> Build()
        {
            if (!File.Exists(configPatn))
            {
                var error = $"Не существует конфигурационный файл {configPatn}";
                Log.Error(error);
                return OperationResult<Config>.Fail(error);
            }

            Config config;
            try
            {
                config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPatn));
            }
            catch (Exception e)
            {
                Log.Error(e);
                return OperationResult<Config>.Fail("Не удалось распарсить конфигурационный файл");
            }

            var allGood = true;

            if (string.IsNullOrEmpty(config.HashCat.Path))
            {
                Log.Message($"{configPatn} не содержит секцию {nameof(config.HashCat.Path)}");
                allGood = false;
            }

            if (!File.Exists(config.HashCat.Path))
            {
                Log.Message($"Hashcat не найден по пути {config.HashCat.Path}");
                allGood = false;
            }

            if (string.IsNullOrEmpty(config.ServerUrl))
            {
                Log.Message($"{configPatn} не содержит секцию {nameof(config.ServerUrl)}");
                allGood = false;
            }

            if (!config.InventoryCheckPeriod.HasValue)
            {
                Log.Message($"{configPatn} не содержит секцию {nameof(config.InventoryCheckPeriod)}");
                allGood = false;
            }

            if (!config.HearbeatPeriod.HasValue)
            {
                Log.Message($"{configPatn} не содержит секцию {nameof(config.HearbeatPeriod)}");
                allGood = false;
            }

            if (!allGood)
                return OperationResult<Config>.Fail("Не удалось получить корректный конфигурационный файл");

            return OperationResult<Config>.Success(config);
        }

        public OperationResult Save(Config config)
        {
            try
            {
                File.WriteAllText(configPatn, JsonSerializer.Serialize(config));
                return OperationResult.Success;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return OperationResult.Fail("Не удалось сохранить конфигруационный файл");
            }
        }
    }
}