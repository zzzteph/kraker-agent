using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public record DoNothingJob() : AbstractJob(JobType.DoNothing);
}