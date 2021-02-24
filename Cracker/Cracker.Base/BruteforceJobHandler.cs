using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    public interface IBruteforceJobHandler:IJobHandler{}
    public class BruteforceJobHandler:IBruteforceJobHandler
    {
        private readonly IKrakerApi _krakerApi;
        private readonly WorkedFolders _workedFolders;
        private readonly ITempFileManager _tempFileManager;
        private readonly IArgumentsBuilder _argumentsBuilder;
        private readonly string _agentId;
        private readonly ISpeedCalculator _speedCalculator;
        private readonly ILogger _logger;

        public BruteforceJobHandler(
            IKrakerApi krakerApi,
            IWorkedFoldersProvider workedFoldersProvider,
            ITempFileManager tempFileManager, 
            IAgentIdManager agentIdManager, IArgumentsBuilder argumentsBuilder, ISpeedCalculator speedCalculator, ILogger logger)
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
            var bruteforceJob = job as BruteforceJob;
            if (bruteforceJob?.JobId == null)
                return PrepareJobResult.FromError("Got a job with id = null");

            var paths = _tempFileManager.BuildTempFilePaths(_workedFolders.TempFolderPath);
            
            if (string.IsNullOrEmpty(bruteforceJob.Content))
                return PrepareJobResult
                    .FromError(
                        $"Got empty content for a bruteforce job: HashId ={bruteforceJob.HashListId}, JobId={bruteforceJob.JobId}");

            var arguments = _argumentsBuilder.Build(job, paths);
            
            File.WriteAllBytes(paths.HashFile, Convert.FromBase64String(bruteforceJob.Content));
            File.WriteAllBytes(paths.PotFile, Convert.FromBase64String(bruteforceJob.PotContent ?? string.Empty));

            return new PrepareJobResult(job, arguments, paths, true, null);
        }


        public async Task Clear(ExecutionResult executionResult)
        {
            var paths = executionResult.Paths;
            _tempFileManager.SoftDelete(paths.HashFile, Constants.HashFile);
            
            var jobId = (executionResult.Job as BruteforceJob).JobId;
            if (!executionResult.IsSuccessful)
            {
                await _krakerApi.SendJob(_agentId, jobId,
                    JobResponse.FromError(jobId, executionResult.ErrorMessage));
                
                DeleteOutputAndPotfile(paths);
                return;
            }

            var err = executionResult.Errors.FirstOrDefault(e =>
                e.Contains("No hashes loaded") || e.Contains("Unhandled Exception"));

            if (err != null)
            {
                await _krakerApi.SendJob(_agentId, jobId, JobResponse.FromError(jobId, err));
                DeleteOutputAndPotfile(paths);
                return;
            }

            var speed = _speedCalculator.CalculateFact(executionResult.Output);
            if (File.Exists(paths.OutputFile))
            {
                var outfile = Convert.ToBase64String(File.ReadAllBytes(paths.OutputFile));
                var potfile = Convert.ToBase64String(File.ReadAllBytes(paths.PotFile));

                await _krakerApi.SendJob(_agentId, jobId,new (jobId, outfile, potfile, speed, null));
                DeleteOutputAndPotfile(paths);
            }
            else
            {
                _logger.Information("Output file doesn't exist");
                await _krakerApi.SendJob(_agentId, jobId,new(jobId, null, String.Empty, speed, null));
            }
        }

        private void DeleteOutputAndPotfile(TempFilePaths paths)
        {
            _tempFileManager.SoftDelete(paths.OutputFile, Constants.Output);
            _tempFileManager.SoftDelete(paths.PotFile, Constants.PotFile);
        }
    }
}