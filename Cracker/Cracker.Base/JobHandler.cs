using System.Threading;
using Cracker.Base.HashCat;
using Cracker.Base.HttpClient;
using Cracker.Base.HttpClient.Data;

namespace Cracker.Base
{
	public abstract class JobHandler
	{
		public CancellationToken ct { get; }
		protected readonly Settings.Settings settings;
		protected readonly ServerClient serverClient;

		protected JobHandler(Settings.Settings settings)
		{
			this.settings = settings;
			ct = new CancellationToken();
			serverClient = new ServerClient(settings.Config);
		}

		public abstract PrepareJobResult Prepare();
		public abstract void Clear(ExecutionResult executionResult);

		public static JobHandler Create(Job job, Settings.Settings settings)
		{
			switch (job?.Type)
			{
				case JobType.Wordlist:
				case JobType.Mask:
					return new WordlistAndMaskJobHandler(job, settings);
				case JobType.Template:
					return new TemplateJobHandler(job, settings);
				case JobType.HashList:
					return new HashListJobHandler(job, settings);
				case JobType.Speedstat:
					return new SpeedstatsJobHandler(job, settings);
				default:
					return new BadJobHandler($"Странненький templateType: {job?.TemplateType}", settings);
			}
		}
	}
}