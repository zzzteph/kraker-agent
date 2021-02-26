using System;
using System.Linq;
using System.Threading.Tasks;
using Cracker.Base.Domain.AgentId;
using Cracker.Base.Domain.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Model.Responses;
using Cracker.Base.Services;
using Cracker.Base.Settings;

namespace Cracker.Base
{
    public interface ITemplateJobHandler : IJobHandler
    { }

    public class TemplateJobHandler : ITemplateJobHandler
    {
        private readonly IKrakerApi _krakerApi;
        private readonly WorkedFolders _workedFolders;
        private readonly IArgumentsBuilder _argumentsBuilder;
        private readonly string _agentId;

        public TemplateJobHandler(
            IKrakerApi krakerApi,
            IWorkedFoldersProvider workedFoldersProvider,
            IArgumentsBuilder argumentsBuilder,
            IAgentIdManager agentIdManager)
        {
            _krakerApi = krakerApi;
            _argumentsBuilder = argumentsBuilder;
            _workedFolders = workedFoldersProvider.Get();
            _agentId = agentIdManager.GetCurrent().Id;
        }

        public PrepareJobResult Prepare(AbstractJob job) 
            => PrepareJobResult.FromArguments(job, _argumentsBuilder.Build(job, null));

        public async Task Clear(ExecutionResult executionResult)
        {
            var keyspace = executionResult.Output.LastOrDefault(o => o != null);
            var templateId = (executionResult.Job as TemplateJob).TemplateId;

            var keyspaceIsLong = long.TryParse(keyspace, out var keyspaceAsLong);
            await _krakerApi.SendTemplate(_agentId,
                templateId,
                new TemplateResponse(keyspaceIsLong?keyspaceAsLong:0, null)
            );
        }
    }
}