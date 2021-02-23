using System;
using System.IO;
using System.Linq;
using Cracker.Base.Domain.AgentId;
using Cracker.Base.Domain.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Model.Responses;
using Cracker.Base.Services;
using Cracker.Base.Settings;
using Serilog;

namespace Cracker.Base
{
    public interface IWordListJobHandler : IJobHandler
    {
    }

    public class WordListJobHandler : IWordListJobHandler
    {
        private readonly IKrakerApi _krakerApi;
        private readonly ITempFileManager _tempFileManager;
        private readonly WorkedFolders _workedFolders;
        private readonly IArgumentsBuilder _argumentsBuilder;
        private readonly string _agentId;
        private readonly ISpeedCalculator _speedCalculator;
        private readonly ILogger _logger;
        
        public WordListJobHandler( IKrakerApi krakerApi,
            ITempFileManager tempFileManager,
            IWorkedFoldersProvider workedFoldersProvider,
            IArgumentsBuilder argumentsBuilder,
            IAgentIdManager agentIdManager,
            ISpeedCalculator speedCalculator, ILogger logger)
        {
            _krakerApi = krakerApi;
            _tempFileManager = tempFileManager;
            _argumentsBuilder = argumentsBuilder;
            _speedCalculator = speedCalculator;
            _logger = logger;
            _agentId = agentIdManager.GetCurrent().Id;
            _workedFolders = workedFoldersProvider.Get();
        }

        public PrepareJobResult Prepare(AbstractJob job)
        {
            var wordlistJob = job as WordListJob;
            if (wordlistJob?.JobId == null)
                return PrepareJobResult.FromError("Get a wordlist job with id = null");

            var paths = _tempFileManager.BuildTempFilePaths(_workedFolders.TempFolderPath);
            
            if (wordlistJob.Content is null or "")
                return PrepareJobResult.FromError(
                    $"A hashfile is empty: HashId ={wordlistJob.HashListId}, JobId={wordlistJob.JobId}");

            var arguments = _argumentsBuilder.Build(job, paths);
            
            File.WriteAllBytes(paths.HashFile, Convert.FromBase64String(wordlistJob.Content));
            File.WriteAllBytes(paths.PotFile, Convert.FromBase64String(wordlistJob.PotContent??null));
            
            return new PrepareJobResult(job, arguments, paths, true, null);
        }

        public void Clear(ExecutionResult executionResult)
        {
            var paths = executionResult.Paths;
            _tempFileManager.SoftDelete(paths.HashFile, Constants.HashFile);

            var jobId = (executionResult.Job as WordListJob).JobId;
            if (!executionResult.IsSuccessful)
            {
                _krakerApi.SendJob(_agentId, jobId, JobResponse.FromError(jobId, executionResult.ErrorMessage));
                DeleteOutputAndPotfile(paths);
                return;
            }

            var err = executionResult.Errors.FirstOrDefault(e =>
                e.Contains("No hashes loaded") || e.Contains("Unhandled Exception"));
            if (err != null)
            {
                _krakerApi.SendJob(_agentId, jobId, JobResponse.FromError(jobId, err));
                DeleteOutputAndPotfile(paths);
                return;
            }

            var speed = _speedCalculator.CalculateFact(executionResult.Output);
            if (File.Exists(paths.OutputFile))
            {
                var outfile = Convert.ToBase64String(File.ReadAllBytes(paths.OutputFile));
                var potfile = Convert.ToBase64String(File.ReadAllBytes(paths.PotFile));

                _krakerApi.SendJob(_agentId, jobId, new (jobId, outfile, potfile, speed, null));
                DeleteOutputAndPotfile(paths);
            }
            else
            {
                _logger.Information("An output file doesn't exist");
                _krakerApi.SendJob(_agentId, jobId, new (jobId, null, string.Empty, speed, null));
            }
        }
        
        private void DeleteOutputAndPotfile(TempFilePaths paths)
        {
            _tempFileManager.SoftDelete(paths.OutputFile, Constants.Output);
            _tempFileManager.SoftDelete(paths.PotFile, Constants.PotFile);
        }
    }
}