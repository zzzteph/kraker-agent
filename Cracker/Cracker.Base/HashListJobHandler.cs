using System;
using System.IO;
using System.Linq;
using Cracker.Base.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Model.Responses;
using Cracker.Base.Services;

namespace Cracker.Base
{
    public class HashListJobHandler : IJobHandler
    {
        private readonly IKrakerApi _krakerApi;
        private readonly Settings.Settings _settings;

        public HashListJobHandler(Settings.Settings settings,
            IKrakerApi krakerApi)
        {
            _settings = settings;
            _krakerApi = krakerApi;
        }

        public PrepareJobResult Prepare(AbstractJob job)
        {
            var hashListJob = job as HashListJob;
            var agentId = _settings.Config.AgentId;
            if (hashListJob?.HashListId == null)
                return PrepareJobResult.FromError($"Can't get HashListId in the job {job}");

            var hashfile = _krakerApi.GetHashListContent(agentId, hashListJob.HashListId).Result;
            if (hashfile?.Content is null or "")
                return PrepareJobResult.FromError(
                    $"Empty content for the job {job}");
            var paths = _settings.WorkedDirectories.TempDirectoryPath.BuildTempFilePaths();
            File.WriteAllBytes(paths.HashFile, Convert.FromBase64String(hashfile.Content));
            File.WriteAllText(paths.PotFile, string.Empty);

            string arguments;
            try
            {
                arguments = new ArgumentsBuilder(_settings.WorkedDirectories).BuildArguments(job, paths);
            }
            catch (Exception e)
            {
                return PrepareJobResult.FromError(
                    $"Couldn't generate arguments for the job:{job}, Error: {e}");
            }

            return new PrepareJobResult(job, arguments, paths, true, null);
        }

        public void Clear(ExecutionResult executionResult)
        {
            var error = executionResult.Errors.FirstOrDefault(e =>
                e.Contains("No hashes loaded") || e.Contains("Unhandled Exception"));

            var paths = executionResult.Paths;

            if (paths != null)
            {
                paths.HashFile.SoftDelete("hashfile");
                paths.PotFile.SoftDelete("potfile");
            }

            if (error != null)
                if (error.Contains("No hashes loaded"))
                    _krakerApi.SendHashList(_settings.Config.AgentId,
                        (executionResult.Job as HashListJob).HashListId,
                        new HashListResponse(0, "No hashes loaded"));
                else
                    _krakerApi.SendHashList(_settings.Config.AgentId,
                        (executionResult.Job as HashListJob).HashListId,
                        new HashListResponse(0, error));
            else
                _krakerApi.SendHashList(_settings.Config.AgentId,
                    (executionResult.Job as HashListJob).HashListId,
                    new HashListResponse(executionResult.Output.Count, null));
        }
    }
}