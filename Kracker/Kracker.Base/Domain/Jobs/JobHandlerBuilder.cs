using Kracker.Base.Domain.Folders;
using Kracker.Base.Domain.HashCat;
using Kracker.Base.Services;
using Kracker.Base.Services.Model.Jobs;
using Serilog;

namespace Kracker.Base.Domain.Jobs
{
    public interface IJobHandlerBuilder
    {
        public IJobHandler BuildBruteforce(AbstractJob job);
        public IJobHandler BuildHashList(AbstractJob job);
        public IJobHandler BuildTemplate(AbstractJob job);
        public IJobHandler BuildWordlist(AbstractJob job);
        public IJobHandler BuildSpeedStat(AbstractJob job);
    }
    
    public class JobHandlerBuilder : IJobHandlerBuilder
    {
        private readonly string _agentId;
        private readonly IHashCatCommandExecutorBuilder _executorBuilder;
        private readonly IKrakerApi _krakerApi;
        private readonly ILogger _logger;
        private readonly ISpeedCalculator _speedCalculator;
        private readonly ITempFileManager _tempFileManager;
        private readonly WorkedFolders _workedFolders;

        public JobHandlerBuilder(IKrakerApi krakerApi,
            WorkedFolders workedFolders,
            ITempFileManager tempFileManager,
            string agentId,
            ISpeedCalculator speedCalculator,
            ILogger logger,
            IHashCatCommandExecutorBuilder executorBuilder)
        {
            _krakerApi = krakerApi;
            _workedFolders = workedFolders;
            _tempFileManager = tempFileManager;
            _agentId = agentId;
            _speedCalculator = speedCalculator;
            _logger = logger;
            _executorBuilder = executorBuilder;
        }
        
        public IJobHandler BuildBruteforce(AbstractJob job)
            => new BruteforceJobHandler(_krakerApi,
                _workedFolders.TempFolderPath,
                _tempFileManager,
                _agentId,
                _speedCalculator,
                _logger,
                job as BruteforceJob,
                _executorBuilder);

        public IJobHandler BuildHashList(AbstractJob job)
            => new HashListJobHandler(_krakerApi,
                _agentId,
                _workedFolders.TempFolderPath,
                _tempFileManager,
                job as HashListJob,
                _executorBuilder,
                _logger);

        public IJobHandler BuildTemplate(AbstractJob job)
            => new TemplateJobHandler(job as TemplateJob, _krakerApi, _executorBuilder, _agentId);


        public IJobHandler BuildWordlist(AbstractJob job)
            => new WordListJobHandler(job as WordListJob, _krakerApi, _tempFileManager, _workedFolders.TempFolderPath,
                _executorBuilder, _agentId, _speedCalculator, _logger);


        public IJobHandler BuildSpeedStat(AbstractJob job)
            => new SpeedstatsJobHandler(_krakerApi, _speedCalculator, _agentId, _executorBuilder, job as SpeedStatJob);
    }
}