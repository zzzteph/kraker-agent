using System.Text.Json.Serialization;

namespace Cracker.Base.Model
{
    public record TemplateWordListJob(long TemplateId,
        string Wordlist,
        string? Rule) : TemplateJob(TemplateId, JobType.TemplateWordlist)
    {
        [JsonPropertyName("template_id")]
        public long TemplateId { get; init; } = TemplateId;
    }
}