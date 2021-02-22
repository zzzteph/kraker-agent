namespace Cracker.Base.Model
{
    public record TemplateWordListJob(long TemplateId,
        string Wordlist,
        string? Rule) : TemplateJob(TemplateId, JobType.TemplateWordlist);
}