using System.Text.Json.Serialization;
using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public record HashListJob(long HashListId, int HashTypeId, string Content) : AbstractJob(JobType.HashList)
    {
        [JsonPropertyName("hashlist_id")]
        public long HashListId { get; init; } = HashListId;
        
        [JsonPropertyName("hashtype_id")]
        public int HashTypeId { get; init; } = HashTypeId;
    }
}