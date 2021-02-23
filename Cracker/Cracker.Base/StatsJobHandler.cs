using Cracker.Base.Domain.AgentId;
using Cracker.Base.Domain.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Model.Responses;
using Cracker.Base.Services;

namespace Cracker.Base
{
    public interface ISpeedstatsJobHandler : IJobHandler
    {
    }

    public class SpeedstatsJobHandler : ISpeedstatsJobHandler
    {
        private readonly IKrakerApi _krakerApi;
        private readonly ISpeedCalculator _speedCalculator;
        private readonly string _agentId;
        public SpeedstatsJobHandler(
            IKrakerApi krakerApi,
            ISpeedCalculator speedCalculator,
            IAgentIdManager agentIdManager)
        {
            _krakerApi = krakerApi;
            _speedCalculator = speedCalculator;
            _agentId = agentIdManager.GetCurrent().Value;
        }

        public PrepareJobResult Prepare(AbstractJob job) 
            => PrepareJobResult.FromArguments($"-b -m {(job as SpeedStatJob).HashTypeId} --machine-readable");

        public void Clear(ExecutionResult executionResult)
        {
            var speed = _speedCalculator.CalculateBenchmark(executionResult.Output);
            
            var hashTypeId = (executionResult.Job as SpeedStatJob).HashTypeId;
            var stat = new SpeedStatResponse(hashTypeId, speed.ToString());

            _krakerApi.SendSpeedStats(_agentId,
                hashTypeId,
                stat);
        }
    }
}