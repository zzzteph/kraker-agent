using System.Threading.Tasks;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Model.Responses;
using Refit;

namespace Cracker.Base.Services
{
    public interface IKrakerApi
    {
        [Post("/api/agents/")]
        Task<AgentId> RegisterAgent();
        
        [Post("/api/agents/{agent_id}/info")]
        Task SendAgentInfo([AliasAs("agent_id")][Query] string agentId, [Body] AgentInfo agentInfo);
        
        [Post("/api/agents/{agent_id}/inventory")]
        Task SendAgentInventory([AliasAs("agent_id")][Query] string agentId,[Body] FileDescription[] fileDescriptions);
        
        [Post("/api/agents/{agent_id}/status")]
        Task<WorkStatus> SendAgentStatus([AliasAs("agent_id")][Query] string agentId);

        [Get("/api/works")]
        Task<AbstractJob?> GetJob([AliasAs("agent_id")][Query] string agentId);

        [Put("/api/agents/{agent_id}/speedstats")]
        Task SendSpeedStats([AliasAs("agent_id")][Query] string agentId,
            [Body] SpeedStatResponse speedStat);

        
        [Put("/api/hashlists/{hashlist_id}")]
        Task SendHashList([AliasAs("agent_id")][Query] string agentId,
            [AliasAs("hashlist_id")] string hashlistId,
            [Body] HashListResponse hashListResponse);
        
        
        [Put("/api/templates/{template_id}")]
        Task SendTemplate([AliasAs("agent_id")][Query] string agentId,
            [AliasAs("template_id")] long templateId,
            [Body] TemplateResponse templateResponse );
        
        [Post("/api/jobs/{job_id}")]
        Task SendJob([AliasAs("agent_id")][Query] string agentId,
            [AliasAs("job_id")] long jobId,
            [Body]  JobResponse jobResponse );
        
    }
}