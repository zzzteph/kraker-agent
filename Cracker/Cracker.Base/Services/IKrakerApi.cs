using System.Threading.Tasks;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Model.Responses;
using Refit;

namespace Cracker.Base.Services
{
    public interface IKrakerApi
    {
        [Post("api/agents/")]
        Task<string> RegisterAgent();
        
        [Post("api/agents/{agent_id}/info")]
        Task<string> SendAgentInfo([AliasAs("agent_id")][Query] string agentId, [Body] AgentInfo agentInfo);
        
        [Post("api/agents/{agent_id}/inventory")]
        Task<string> SendAgentInventory([AliasAs("agent_id")][Query] string agentId,[Body] FileDescription[] fileDescriptions);
        
        [Post("api/agents/{agent_id}/status")]
        Task<WorkStatus> SendAgentStatus([AliasAs("agent_id")][Query] string agentId);

        //[Post("api/agents/{agent_id}/speedstat")]
        //Task<string> SendSpeedstat([AliasAs("agent_id")][Query] string agentId, [Body] SpeedStat[] speedStat);

        [Get("api/works")]
        Task<AbstractJob> GetJob([AliasAs("agent_id")][Query] string agentId);

        [Post("api/speedstats/{hashtype_id}")]
        Task<AbstractJob> SendSpeedStats([AliasAs("agent_id")][Query] string agentId,
            [AliasAs("hashtype_id")] long hashTypeId,
            [Body] SpeedStatResponse speedStat);

        [Get("api/hashlists/{hashlist_id}/content")]
        Task<ContentHolder> GetHashListContent([AliasAs("agent_id")][Query] string agentId,
            [AliasAs("hashlist_id")] long hashListId);
        
        [Post("api/hashlists/{hashlist_id}")]
        Task SendHashList([AliasAs("agent_id")][Query] string agentId,
            [AliasAs("hashlist_id")] long hashlistId,
            [Body] HashListResponse hashListResponse);
        
        
        [Get("api/pot/{hashlist_id}/content")]
        Task<ContentHolder> GetPotFileContent([AliasAs("agent_id")][Query] string agentId,
            [AliasAs("hashlist_id")] long hashListId);

        [Post("api/templates/{template_id}")]
        Task<AbstractJob> SendTemplate([AliasAs("agent_id")][Query] string agentId,
            [AliasAs("template_id")] long templateId,
            [Body] TemplateResponse templateResponse );
        
        [Post("api/jobs/{job_id}")]
        Task<AbstractJob> SendJob([AliasAs("agent_id")][Query] string agentId,
            [AliasAs("job_id")] long jobId,
            [Body]  JobResponse jobResponse );
        
    }
}