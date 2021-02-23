﻿using System;
using System.IO;
using System.Linq;
using Cracker.Base.Domain.AgentId;
using Cracker.Base.Domain.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Model.Responses;
using Cracker.Base.Services;
using Cracker.Base.Settings;

namespace Cracker.Base
{
    public class HashListJobHandler : IJobHandler
    {
        private readonly IKrakerApi _krakerApi;
        private readonly string _agentId;
        private readonly WorkedFolders _workedFolders;
        private readonly ITempFileManager _tempFileManager;
        private readonly IArgumentsBuilder _argumentBuilder;
        public HashListJobHandler(
            IKrakerApi krakerApi,
            IAgentIdManager agentIdManager,
            IWorkedFoldersProvider workedFoldersProvider,
            ITempFileManager tempFileManager, 
            IArgumentsBuilder argumentBuilder)
        {
            _krakerApi = krakerApi;
            _tempFileManager = tempFileManager;
            _argumentBuilder = argumentBuilder;
            _workedFolders = workedFoldersProvider.Get();
            _agentId = agentIdManager.GetCurrent().Value;
        }

        public PrepareJobResult Prepare(AbstractJob job)
        {
            var hashListJob = job as HashListJob;
            if (hashListJob?.HashListId == null)
                return PrepareJobResult.FromError($"Can't get HashListId in the job {job}");

            if (hashListJob.Content is null or "")
                return PrepareJobResult.FromError(
                    $"Empty content for the job {job}");
            var paths = _tempFileManager.BuildTempFilePaths(_workedFolders.TempFolderPath);
            File.WriteAllBytes(paths.HashFile, Convert.FromBase64String(hashListJob.Content));
            File.WriteAllText(paths.PotFile, string.Empty);

            var arguments = _argumentBuilder.Build(job, paths);
            
            return new PrepareJobResult(job, arguments, paths, true, null);
        }

        public void Clear(ExecutionResult executionResult)
        {
            var error = executionResult.Errors.FirstOrDefault(e =>
                e.Contains("No hashes loaded") || e.Contains("Unhandled Exception"));

            var paths = executionResult.Paths;

            if (paths != null)
            {
                _tempFileManager.SoftDelete(paths.OutputFile, Constants.Output);
                _tempFileManager.SoftDelete(paths.PotFile, Constants.PotFile);
            }

            if (error != null)
                if (error.Contains("No hashes loaded"))
                    _krakerApi.SendHashList(_agentId,
                        (executionResult.Job as HashListJob).HashListId,
                        new HashListResponse(0, "No hashes loaded"));
                else
                    _krakerApi.SendHashList(_agentId,
                        (executionResult.Job as HashListJob).HashListId,
                        new HashListResponse(0, error));
            else
                _krakerApi.SendHashList(_agentId,
                    (executionResult.Job as HashListJob).HashListId,
                    new HashListResponse(executionResult.Output.Count, null));
        }
    }
}