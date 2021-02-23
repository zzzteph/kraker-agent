using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public record WordListJob(long JobId,
        long HashListId,
        int HashTypeId,
        long Skip,
        long Limit,
        string Wordlist,
        string Rule,
        string Content,
        string PotContent) : AbstractJob(JobType.WordList);
}