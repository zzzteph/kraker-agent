using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public record HashListJob(long HashListId, int HashTypeId) : AbstractJob(JobType.HashList);
}