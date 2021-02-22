using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public record SpeedStatJob(long HashTypeId) : AbstractJob(JobType.SpeedStat);
}