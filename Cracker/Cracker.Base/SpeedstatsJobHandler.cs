using System.Threading.Tasks;
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
        private readonly IArgumentsBuilder _argumentsBuilder;
        public SpeedstatsJobHandler(
            IKrakerApi krakerApi,
            ISpeedCalculator speedCalculator,
            IAgentIdManager agentIdManager,
            IArgumentsBuilder argumentsBuilder)
        {
            _krakerApi = krakerApi;
            _speedCalculator = speedCalculator;
            _argumentsBuilder = argumentsBuilder;
            _agentId = agentIdManager.GetCurrent().Id;
        }

        public PrepareJobResult Prepare(AbstractJob job) 
            => PrepareJobResult.FromArguments(job, _argumentsBuilder.Build(job, null));

        public async Task Clear(ExecutionResult executionResult)
        {
            var speed = _speedCalculator.CalculateBenchmark(executionResult.Output);
            
            var hashTypeId = (executionResult.Job as SpeedStatJob).HashTypeId;
            var stat = new SpeedStatResponse(hashTypeId, speed.ToString());

            await _krakerApi.SendSpeedStats(_agentId,
                stat);
        }
    }
}