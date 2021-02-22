using Cracker.Base.HashCat;
using Cracker.Base.Model.Jobs;

namespace Cracker.Base
{
    public interface IJobHandler
    {
        PrepareJobResult Prepare(AbstractJob job);
        void Clear(ExecutionResult executionResult);
    }
}