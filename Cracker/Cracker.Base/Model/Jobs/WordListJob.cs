using System.Text.Json.Serialization;
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
        string PotContent) : AbstractJob(JobType.WordList)
    {
        [JsonPropertyName("job_id")] public long JobId { get; init; } = JobId;

        [JsonPropertyName("hashlist_id")] public long HashListId { get; init; } = HashListId;

        [JsonPropertyName("hashtype_id")] public int HashTypeId { get; init; } = HashTypeId;

        [JsonPropertyName("pot_content")] public string PotContent { get; init; } = PotContent;
    }
}