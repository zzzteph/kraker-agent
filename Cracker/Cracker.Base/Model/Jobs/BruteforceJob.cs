using System.Text.Json.Serialization;
using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public record BruteforceJob(long JobId,
        long HashListId,
        int HashTypeId,
        long Skip,
        long Limit,
        string Mask,
        string Charset1,
        string Charset2,
        string Charset3,
        string Charset4,
        string Content,
        string PotContent) : AbstractJob(JobType.Bruteforce)
    {
        [JsonPropertyName("job_id")] public long JobId { get; init; } = JobId;

        [JsonPropertyName("hashlist_id")] public long HashListId { get; init; } = HashListId;

        [JsonPropertyName("hashtype_id")] public int HashTypeId { get; init; } = HashTypeId;

        [JsonPropertyName("pot_content")] public string PotContent { get; init; } = PotContent;
    }
}