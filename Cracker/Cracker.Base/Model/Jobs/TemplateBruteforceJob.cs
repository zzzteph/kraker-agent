using System.Text.Json.Serialization;

namespace Cracker.Base.Model
{
    public record TemplateBruteforceJob(long TemplateId,
        string Mask,
        string Charset1,
        string Charset2,
        string Charset3,
        string Charset4) : TemplateJob(TemplateId, JobType.TemplateBruteforce)
    {
        [JsonPropertyName("template_id")]
        public long TemplateId { get; init; } = TemplateId;
    }
}