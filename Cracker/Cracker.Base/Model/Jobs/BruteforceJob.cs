using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public record BruteforceJob(long JobId,
        long HashlistId,
        int HashListTypeId,
        long Skip,
        long Limit,
        string Mask,
        string Charset1,
        string Charset2,
        string Charset3,
        string Charset4) : AbstractJob(JobType.Bruteforce);
}