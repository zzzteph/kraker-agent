using System;
using System.Linq;
using Cracker.Base.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Model.Responses;
using Cracker.Base.Services;

namespace Cracker.Base
{
    public interface ITemplateJobHandler : IJobHandler
    { }

    public class TemplateJobHandler : ITemplateJobHandler
    {
        private readonly Settings.Settings _settings;
        private readonly IKrakerApi _krakerApi;

        public TemplateJobHandler(Settings.Settings settings,
            IKrakerApi krakerApi)
        {
            _settings = settings;
            _krakerApi = krakerApi;
        }

        public PrepareJobResult Prepare(AbstractJob job)
        {
            try
            {
                var arguments = new ArgumentsBuilder(_settings.WorkedDirectories).BuildArguments(job, null);

                return PrepareJobResult.FromArguments(arguments);
            }
            catch (Exception e)
            {
                return PrepareJobResult.FromError($"Can't generate arguments for the job:{job}, Error: {e}");
            }
        }

        public void Clear(ExecutionResult executionResult)
        {
            var keyspace = executionResult?.Output.LastOrDefault(o => o != null);
            var templateId = (executionResult.Job as TemplateJob).TemplateId;

            _krakerApi.SendTemplate(_settings.Config.AgentId,
                templateId,
                new TemplateResponse(long.Parse(keyspace), null)
            );
        }
    }
}