using Cracker.Lib.HashCat;
using Cracker.Lib.HttpClient.Data;

namespace Cracker.Lib
{
	public class SpeedstatsJobHandler:JobHandler
	{
		private TempFilePaths paths;
		private readonly Job job;
		public SpeedstatsJobHandler(Job job)
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
			ClientProxyProvider.Client.SendAgentSpeedStats(stat).ConfigureAwait(false);
		}
	}
}