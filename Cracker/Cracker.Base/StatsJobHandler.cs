using Cracker.Base.HashCat;
using Cracker.Base.HttpClient.Data;

namespace Cracker.Base
{
	public class SpeedstatsJobHandler:JobHandler
	{
		private readonly Job job;
		public SpeedstatsJobHandler(Job job, Settings.Settings settings) : base(settings)
		{
			this.job = job;
		}
		public override PrepareJobResult Prepare()
		{
			return new PrepareJobResult
			{
				IsReadyForExecution = true,
				HashCatArguments = $"-b -m {job.Command.M} --machine-readable"
			};
		}

		public override void Clear(ExecutionResult executionResult)
		{
			var speed = SpeedCalculator.CalculateBenchmark(executionResult.Output);
			var stat = new SpeedStat { Mode = job.Command.M, Speed = speed.ToString() };
			serverClient.SendAgentSpeedStats(stat).ConfigureAwait(false);
		}
	}
}