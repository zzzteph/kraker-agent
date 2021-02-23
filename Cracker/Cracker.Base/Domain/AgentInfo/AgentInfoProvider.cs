using System;

namespace Cracker.Base.Domain.AgentInfo
{
    public interface IAgentInfoProvider
    {
        Model.AgentInfo Get();
    }

    public class AgentInfoProvider : IAgentInfoProvider
    {
        private readonly Lazy<Model.AgentInfo> _agentInfo;

        public AgentInfoProvider(IAgentInfoManager manager)
        {
            _agentInfo = new Lazy<Model.AgentInfo>(()=>manager.Build().Result);
        }
        
        public Model.AgentInfo Get() => _agentInfo.Value;
    }
}