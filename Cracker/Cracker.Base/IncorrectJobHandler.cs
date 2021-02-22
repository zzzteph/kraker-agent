using System;
using Cracker.Base.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;

namespace Cracker.Base
{
    public interface IIncorrectJobHandler : IJobHandler
    { }

    public class IncorrectJobHandler : IIncorrectJobHandler
    {
        public PrepareJobResult Prepare(AbstractJob job) =>
            job is IncorrectJob ij
                ? PrepareJobResult.FromError(ij.Error)
                : PrepareJobResult.FromError($"Can't work with the job {job}");

        public void Clear(ExecutionResult executionResult)
        {
            throw new InvalidOperationException($"Can't work with the job: {executionResult.ErrorMessage}");
        }
    }
}