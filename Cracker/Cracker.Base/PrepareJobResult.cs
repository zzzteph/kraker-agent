using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;

namespace Cracker.Base
{
    public record PrepareJobResult(AbstractJob? Job,
        string HashCatArguments,
        TempFilePaths? Paths,
        bool IsReadyForExecution,
        string Error
    )
    {
        public static PrepareJobResult FromArguments(string hashCatArguments)
            => new PrepareJobResult(null, hashCatArguments, null, true, null);

        public static PrepareJobResult FromArguments(AbstractJob job, string hashCatArguments)
            => new PrepareJobResult(job, hashCatArguments, null, true, null);
        
        public static PrepareJobResult FromError(string error)
            => new PrepareJobResult(null, null, null, false, error);
    }
}