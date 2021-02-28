using System;
using System.Collections.Generic;
using Kracker.Base.Services.Model.Jobs;

namespace Kracker.Base.Domain.Jobs
{
    public interface IJobHandlerProvider
    {
        IJobHandler Get(AbstractJob job);
    }
    
    public class JobHandlerProvider : IJobHandlerProvider
    {
        private readonly IIncorrectJobHandler _incorrectJobHandler;
        private readonly IReadOnlyDictionary<JobType, Func<AbstractJob, IJobHandler>> _map;

        public JobHandlerProvider(IJobHandlerBuilder jobHandlerBuilder,
            IIncorrectJobHandler incorrectJobHandler)
        {
            _incorrectJobHandler = incorrectJobHandler;
            _map = new Dictionary<JobType, Func<AbstractJob, IJobHandler>>
            {
                {JobType.Bruteforce, jobHandlerBuilder.BuildBruteforce},
                {JobType.HashList, jobHandlerBuilder.BuildHashList},
                {JobType.SpeedStat, jobHandlerBuilder.BuildSpeedStat},
                {JobType.TemplateBruteforce, jobHandlerBuilder.BuildTemplate},
                {JobType.TemplateWordlist, jobHandlerBuilder.BuildTemplate},
                {JobType.WordList, jobHandlerBuilder.BuildWordlist}
            };
        }

        public IJobHandler Get(AbstractJob job) =>
            _map.TryGetValue(job.Type, out var handlerBuilder)
                ? handlerBuilder(job)
                : _incorrectJobHandler;
    }
}