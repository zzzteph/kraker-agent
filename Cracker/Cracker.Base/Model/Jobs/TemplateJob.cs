using System.Text.Json.Serialization;
using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public abstract record TemplateJob(long TemplateId, JobType Type):AbstractJob(Type)
    {
        [JsonPropertyName("template_id")]
        public long TemplateId { get; init; } = TemplateId;
    }
}