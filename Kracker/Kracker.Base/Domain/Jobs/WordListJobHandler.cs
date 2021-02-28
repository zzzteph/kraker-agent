using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kracker.Base.Domain.HashCat;
using Kracker.Base.Services;
using Kracker.Base.Services.Model.Jobs;
using Kracker.Base.Services.Model.Responses;
using Serilog;

namespace Kracker.Base.Domain.Jobs
{
    public class WordListJobHandler : JobHandler<WordListJob>
    {
        private readonly ITempFileManager _tempFileManager;
        private readonly ISpeedCalculator _speedCalculator;
        private readonly ILogger _logger;
        
        public WordListJobHandler(
            WordListJob job,
            IKrakerApi krakerApi,
            ITempFileManager tempFileManager,
            string tempFilesPath,
            IHashCatCommandExecutorBuilder executorBuilder,
            string agentId,
            ISpeedCalculator speedCalculator, ILogger logger)
        :base(job, agentId, tempFileManager.BuildTempFilePaths(tempFilesPath),
            executorBuilder, krakerApi)
        {
            _tempFileManager = tempFileManager;
            _speedCalculator = speedCalculator;
            _logger = logger;
            
            _tempFileManager.WriteBase64Content(_paths.HashFile, _job.Content);
            _tempFileManager.WriteBase64Content(_paths.PotFile, _job.PotContent ?? string.Empty);
        }
        public override async Task Clear()
        {
            var executionResult = _hashCatTask.Result;
            
            _tempFileManager.SoftDelete(_paths.HashFile, Constants.HashFile);

            var jobId = _job.JobId;
            if (!executionResult.IsSuccessful)
            {
                await _krakerApi.SendJob(_agentId, jobId, JobResponse.FromError(jobId, executionResult.ErrorMessage));
                _tempFileManager.DeleteOutputAndPotfile(_paths);
                return;
            }

            var err = executionResult.Errors.FirstOrDefault(e =>
                e.Contains("No hashes loaded") || e.Contains("Unhandled Exception"));
            if (err != null)
            {
                await _krakerApi.SendJob(_agentId, jobId, JobResponse.FromError(jobId, err));
                _tempFileManager.DeleteOutputAndPotfile(_paths);
                return;
            }

            var speed = _speedCalculator.CalculateFact(executionResult.Output);
            if (File.Exists(_paths.OutputFile))
            {
                var outfile = Convert.ToBase64String(File.ReadAllBytes(_paths.OutputFile));
                var potfile = Convert.ToBase64String(File.ReadAllBytes(_paths.PotFile));

                await _krakerApi.SendJob(_agentId, jobId, new (jobId, outfile, potfile, (long) speed, null));
                _tempFileManager.DeleteOutputAndPotfile(_paths);
                
            }
            else
            {
                _logger.Information("An output file doesn't exist");
                await _krakerApi.SendJob(_agentId, jobId, new (jobId, null, string.Empty, (long) speed, null));
            }
        }
    }
}