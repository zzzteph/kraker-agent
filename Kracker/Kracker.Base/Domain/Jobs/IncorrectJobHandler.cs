using System;
using System.Threading.Tasks;
using Kracker.Base.Services.Model.Jobs;

namespace Kracker.Base.Domain.Jobs
{
    public interface IIncorrectJobHandler : IJobHandler
    { }

    public class IncorrectJobHandler : IIncorrectJobHandler
    {
        private readonly IncorrectJob _job;
        public IncorrectJobHandler(IncorrectJob job)
        {
            _job = job;
        }
        public Task Clear()
        {
            throw new InvalidOperationException($"Can't work with the job: {_job}");
        }

        public void Execute()
        {
            throw new InvalidOperationException($"Can't work with the job: {_job}");
        }

        public bool IsCompleted()
        {
            throw new InvalidOperationException($"Can't work with the job: {_job}");
        }

        public JobDescription GetJobDescription()
        {
            throw new InvalidOperationException($"Can't work with the job: {_job}");
        }

        public void Cancel()
        {
            throw new InvalidOperationException($"Can't work with the job: {_job}");
        }
    }
}