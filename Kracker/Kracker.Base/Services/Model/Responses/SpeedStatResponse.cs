using System.Text.Json.Serialization;

namespace Kracker.Base.Services.Model.Responses
{
    public record SpeedStatResponse(long HashTypeId, string Speed)
    {
        [JsonPropertyName("hashtype_id")]
        public long HashTypeId { get; init; } = HashTypeId;
        
        [JsonPropertyName("speed")]
        public string Speed { get; init; } = Speed;
    }
}