using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public abstract record TemplateJob(long TemplateId, JobType Type):AbstractJob(Type);
}