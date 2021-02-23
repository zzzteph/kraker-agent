using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cracker.Base.Model;
using Cracker.Base.Settings;
using Serilog;
using static Cracker.Base.Model.Constants;

namespace Cracker.Base.Domain.Inventory
{
    public interface IInventoryManager
    {
        OperationResult<Inventory> Initialize();
        (bool wasChanged, Inventory Inventory) UpdateFileDescriptions();
    }

    public class InventoryManager : IInventoryManager
    {
        private readonly IFileDescriptionBuilder _descriptionBuilder;
        private readonly string _inventoryFilePath;
        private readonly ILogger _logger;
        private readonly WorkedFolders _workedFolders;

        private Dictionary<string, FileDescription> _fileDescriptions;
        private Inventory _currentInventory;

        public InventoryManager(
            AppFolder appFolder,
            ILogger logger,
            IWorkedFoldersProvider workedFoldersProvider,
            IFileDescriptionBuilder descriptionBuilder)
        {
            _logger = logger;
            _descriptionBuilder = descriptionBuilder;
            _workedFolders = workedFoldersProvider.Get();
            _inventoryFilePath = Path.Combine(appFolder.Value, ArtefactsFolder, InventoryFile);
        }

        public OperationResult<Inventory> Initialize()
        {
            try
            {
                _fileDescriptions = Directory.GetFiles(_workedFolders.RulesPath)
                    .Concat(Directory.GetFiles(_workedFolders.WordlistPath))
                    .ToDictionary(p => p, _descriptionBuilder.Build);

                File.WriteAllText(_inventoryFilePath, JsonSerializer.Serialize(_fileDescriptions));

                _currentInventory = new Inventory(_fileDescriptions);
                return OperationResult<Inventory>.Success(_currentInventory);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Can't prepare inventory");
                return OperationResult<Inventory>.Fail(
                    "[Inventory] Can't prepare inventory");
            }
        }


       
        public (bool wasChanged, Inventory Inventory) UpdateFileDescriptions()
        {
            _logger.Information("[inventory] Time to check inventory!");

            var isChanged = false;
            
            var currentFiles = Directory.GetFiles(_workedFolders.RulesPath)
                .Concat(Directory.GetFiles(_workedFolders.WordlistPath))
                .ToList();

            foreach (var currentFile in currentFiles)
            {
                if (_fileDescriptions.TryGetValue(currentFile, out var oldFileDescription)
                    && File.GetLastWriteTime(currentFile) == oldFileDescription.LastWriteTime)
                    continue;
                try
                {
                    _logger.Information($"[inventory] Have detected a new filе: {currentFile}");
                    _fileDescriptions[currentFile] = _descriptionBuilder.Build(currentFile);
                    isChanged = true;
                }
                catch (Exception e)
                {
                    _logger.Information($"[inventory] can't calculate fileDescription for {currentFile}: {e}");
                }
            }

            var deletedFiles = _fileDescriptions.Keys.ToHashSet();
            deletedFiles.ExceptWith(currentFiles);

            foreach (var deletedFile in deletedFiles)
            {
                _logger.Information($"[inventory] Have detected removing of file: {deletedFile}");
                _fileDescriptions.Remove(deletedFile);
                isChanged = true;
            }

            if (isChanged)
            {
                _logger.Information("[inventory] Changes've detected, save data");
                File.WriteAllText(_inventoryFilePath, JsonSerializer.Serialize(_fileDescriptions));
                _currentInventory = new Inventory(_fileDescriptions);
            }
            else
            {
                _logger.Information("[inventory] Checking has finished, changes've not detected");
            }

            return (isChanged, _currentInventory);
        }
    }
}