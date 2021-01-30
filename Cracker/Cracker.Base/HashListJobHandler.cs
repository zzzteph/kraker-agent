using System;
using System.IO;
using System.Linq;
using Cracker.Base.HashCat;
using Cracker.Base.HttpClient.Data;

namespace Cracker.Base
{
    public class HashListJobHandler : JobHandler
    {
        private readonly Job job;
        private readonly TempFilePaths paths;

        public HashListJobHandler(Job job, Settings.Settings settings) : base(settings)
        {
            this.job = job;
            paths = settings.WorkedDirectories.TempDirectoryPath.BuildTempFilePaths();
        }

        public override PrepareJobResult Prepare()
        {
            if (job.HashId == null)
                return new PrepareJobResult
                {
                    IsReadyForExecution = false,
                    Error = $"Нет hashId у работы с типом {job.Type}"
                };

            var hashfile = serverClient.GetHashFile(job.HashId).Result;
            if (string.IsNullOrEmpty(hashfile?.Data))
                return new PrepareJobResult
                {
                    Error = $"Пустой hashfile: HashId ={job.HashId}, JobId={job.JobId}",
                    IsReadyForExecution = false
                };
            File.WriteAllBytes(paths.HashFile, Convert.FromBase64String(hashfile.Data));
            File.WriteAllText(paths.PotFile, string.Empty);

            string arguments;
            try
            {
                arguments = new ArgumentsBuilder(settings.WorkedDirectories).BuildArguments(job, paths);
            }
            catch (Exception e)
            {
                return new PrepareJobResult
                {
                    Error = $"Не смогли сгенерить аргументы для задачи JobId:{job.JobId}, Error: {e}",
                    IsReadyForExecution = false
                };
            }

            return new PrepareJobResult
            {
                IsReadyForExecution = true,
                HashCatArguments = arguments
            };
        }

        public override void Clear(ExecutionResult executionResult)
        {
            var error = executionResult.Errors.FirstOrDefault(e =>
                e.Contains("No hashes loaded") || e.Contains("Unhandled Exception"));

            if (paths != null)
            {
                paths.HashFile.SoftDelete("hashfile");
                paths.PotFile.SoftDelete("potfile");
            }

            if (error != null)
                if (error.Contains("No hashes loaded"))
                    serverClient.Client.PostAsync<object>(
                        $"api/hash/update/{settings.Config.RegistrationKey}/{job.HashId}",
                        () => new {keyspace = 0}).ConfigureAwait(false);
                else
                    serverClient.Client.PostAsync<object>(
                        $"api/hash/update/{settings.Config.RegistrationKey}/{job.HashId}",
                        () => new {error}).ConfigureAwait(false);
            else
                serverClient.Client.PostAsync<object>(
                    $"api/hash/update/{settings.Config.RegistrationKey}/{job.HashId}",
                    () => new {keyspace = executionResult.Output.Count}).ConfigureAwait(false);
        }
    }
}