using System.Text.Json.Serialization;
using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public record SpeedStatJob(long HashTypeId) : AbstractJob(JobType.SpeedStat)
    {
        [JsonPropertyName("hashtype_id")]
        public long HashTypeId { get; init; } = HashTypeId;
    }
}