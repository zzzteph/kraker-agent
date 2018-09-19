using System.Threading;
using Cracker.Base.HashCat;
using Cracker.Base.HttpClient.Data;

namespace Cracker.Base
{
	public abstract class JobHandler
	{
		public CancellationToken ct { get; }

		protected JobHandler()
		{
			ct = new CancellationToken();
		}

		public abstract PrepareJobResult Prepare();
		public abstract void Clear(ExecutionResult executionResult);

		public static JobHandler Create(Job job)
		{
			switch (job?.Type)
			{
				case JobType.Wordlist:
				case JobType.Mask:
					return new WordlistAndMaskJobHandler(job);
				case JobType.Template:
					return new TemplateJobHandler(job);
				case JobType.HashList:
					return new HashListJobHandler(job);
				case JobType.Speedstat:
					return new SpeedstatsJobHandler(job);
				default:
					return new BadJobHandler($"Странненький templateType: {job?.TemplateType}");
			}
		}
	}
}