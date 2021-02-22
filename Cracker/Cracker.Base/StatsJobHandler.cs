using Cracker.Base.HashCat;
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
        private readonly Settings.Settings _settings;
        private readonly IKrakerApi _krakerApi;

        public SpeedstatsJobHandler(Settings.Settings settings,
            IKrakerApi krakerApi)
        {
            _settings = settings;
            _krakerApi = krakerApi;
        }

        public PrepareJobResult Prepare(AbstractJob job)
        {
            return PrepareJobResult.FromArguments($"-b -m {(job as SpeedStatJob).HashTypeId} --machine-readable");
        }

        public void Clear(ExecutionResult executionResult)
        {
            var speed = SpeedCalculator.CalculateBenchmark(executionResult.Output);
            
            var hashTypeId = (executionResult.Job as SpeedStatJob).HashTypeId;
            var stat = new SpeedStatResponse(hashTypeId, speed.ToString());

            _krakerApi.SendSpeedStats(_settings.Config.AgentId,
                hashTypeId,
                stat);
        }
    }
}