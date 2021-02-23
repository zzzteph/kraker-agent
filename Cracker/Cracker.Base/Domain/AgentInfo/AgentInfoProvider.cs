namespace Cracker.Base.Domain.AgentInfo
{
    public interface IAgentInfoProvider
    {
        Model.AgentInfo Get();
    }

    public class AgentInfoProvider : IAgentInfoProvider
    {
        private readonly Model.AgentInfo _agentInfo;

        public AgentInfoProvider(IAgentInfoManager manager)
        {
            _agentInfo = manager.Build().Result;
        }
        
        public Model.AgentInfo Get() => _agentInfo;
    }
}