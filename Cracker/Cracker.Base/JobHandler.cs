using System.Collections.Generic;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;

namespace Cracker.Base
{
    public interface IJobHandlerProvider
    {
        IJobHandler Get(AbstractJob job);
    }

    public class JobHandlerProvider : IJobHandlerProvider
    {
        private readonly IIncorrectJobHandler _incorrectJobHandler;
        private readonly IReadOnlyDictionary<JobType, IJobHandler> _map;

        public JobHandlerProvider(IBruteforceJobHandler bruteforceJobHandler,
            IIncorrectJobHandler incorrectJobHandler)
        {
            _incorrectJobHandler = incorrectJobHandler;
            _map = new Dictionary<JobType, IJobHandler>
            {
                {JobType.Bruteforce, bruteforceJobHandler}
            };
        }

        public IJobHandler Get(AbstractJob job) =>
            _map.TryGetValue(job.Type, out var handler)
                ? handler
                : _incorrectJobHandler;
    }
}