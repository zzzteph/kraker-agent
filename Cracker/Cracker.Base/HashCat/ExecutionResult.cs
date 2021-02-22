using System.Collections.Generic;
using Cracker.Base.Model.Jobs;

namespace Cracker.Base.HashCat
{
    public record ExecutionResult(int? HashCatExitCode,
        IReadOnlyList<string>? Output,
        IReadOnlyList<string>? Errors,
        TempFilePaths? Paths,
        AbstractJob Job,
        bool IsSuccessful,
        string? ErrorMessage
    )
    {
        public static ExecutionResult FromError(string error) => new ExecutionResult(null,
            null,
            null,
            null,
            null,
            false,
            error);
    }
}