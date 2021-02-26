using System.Text.Json.Serialization;

namespace Cracker.Base.Model
{
    public record TemplateWordListJob(long TemplateId,
        long WordlistId,
        long? RuleId) : TemplateJob(TemplateId, JobType.TemplateWordlist)
    {
        [JsonPropertyName("template_id")]
        public long TemplateId { get; init; } = TemplateId;

        [JsonPropertyName("wordlist_id")]
        public long WordlistId { get; init; } = WordlistId;
        
        [JsonPropertyName("rule_id")]
        public long? RuleId { get; init; } = RuleId;
    }
}