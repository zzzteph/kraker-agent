using System;
using System.IO;
using System.Linq;
using Cracker.Base.HashCat;
using Cracker.Base.Logging;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Services;

namespace Cracker.Base
{
    public interface IWordListJobHandler : IJobHandler
    {
    }

    public class WordListJobHandler : IWordListJobHandler
    {
        private readonly Settings.Settings _settings;
        private readonly IKrakerApi _krakerApi;

        
        public WordListJobHandler(Settings.Settings settings, IKrakerApi krakerApi)
        {
            _settings = settings;
            _krakerApi = krakerApi;
        }

        public PrepareJobResult Prepare(AbstractJob job)
        {
            var wordlistJob = job as WordListJob;
            if (wordlistJob?.JobId == null)
                return PrepareJobResult.FromError("Get a wordlist job with id = null");

            var paths = _settings.WorkedDirectories.TempDirectoryPath.BuildTempFilePaths();
            var hashfile = _krakerApi.GetHashListContent(_settings.Config.AgentId, wordlistJob.HashListId).Result;
            if (hashfile.Content is null or "")
                return PrepareJobResult.FromError(
                    $"A hashfile is empty: HashId ={wordlistJob.HashListId}, JobId={wordlistJob.JobId}");

            string arguments;
            try
            {
                arguments = new ArgumentsBuilder(_settings.WorkedDirectories).BuildArguments(wordlistJob, paths);
            }
            catch (Exception e)
            {
                return PrepareJobResult.FromError(
                    $"Cann't build arguments for a job. JobId:{wordlistJob.JobId}, Error: {e}");
            }


            File.WriteAllBytes(paths.HashFile, Convert.FromBase64String(hashfile.Content));

            var potFile = _krakerApi.GetPotFileContent(_settings.Config.AgentId, wordlistJob.HashListId).Result;

            File.WriteAllBytes(paths.PotFile, Convert.FromBase64String(potFile?.Content ?? string.Empty));
            
            return new PrepareJobResult(job, arguments, paths, true, null);
        }

        public void Clear(ExecutionResult executionResult)
        {
            var paths = executionResult.Paths;
            paths.HashFile.SoftDelete("hashfile");

            if (!executionResult.IsSuccessful)
            {
                paths.OutputFile.SoftDelete("outfile");
                paths.PotFile.SoftDelete("potfile");
                return;
            }

            var err = executionResult.Errors.FirstOrDefault(e =>
                e.Contains("No hashes loaded") || e.Contains("Unhandled Exception"));
            if (err != null)
            {
                //_krakerApi.SendJobEnd(new {error = err}, _job.JobId).ConfigureAwait(false);
                paths.OutputFile.SoftDelete("outfile");
                paths.PotFile.SoftDelete("potfile");
                return;
            }

            var speed = SpeedCalculator.CalculateFact(executionResult.Output);
            if (File.Exists(paths.OutputFile))
            {
                var outfile = Convert.ToBase64String(File.ReadAllBytes(paths.OutputFile));
                var potfile = Convert.ToBase64String(File.ReadAllBytes(paths.PotFile));

                //_krakerApi.SendJobEnd(new {outfile, potfile, speed}, _job.JobId).ConfigureAwait(false);
                paths.OutputFile.SoftDelete("outfile");
                paths.PotFile.SoftDelete("potfile");
            }
            else
            {
                Log.Message("An output file doesn't exist");
                //_krakerApi.SendJobEnd(new {potfile = string.Empty, speed}, _job.JobId).ConfigureAwait(false);
            }
        }
    }
}