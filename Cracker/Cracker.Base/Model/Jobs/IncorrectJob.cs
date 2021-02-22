using Cracker.Base.Model.Jobs;

namespace Cracker.Base.Model
{
    public record IncorrectJob(string Error) : AbstractJob(JobType.UnrecognizedJob);
}