using System.Threading.Tasks;
using Cracker.Base.Domain.AgentId;
using Cracker.Base.Domain.AgentInfo;
using Cracker.Base.Model;
using Cracker.Base.Services;

namespace Cracker.Base.Settings
{
    public interface IAgentRegistrationManager
    {
        Task<(AgentId agentId, AgentInfo agentInfo)> Register();
    }

    public class AgentRegistrationManager : IAgentRegistrationManager
    {
        private readonly IKrakerApi _krakerApi;
        private readonly IAgentInfoManager _agentInfoManager;
        private readonly IAgentInfoProvider _agentInfoProvider;
        private readonly IAgentIdManager _agentIdManager;

        public AgentRegistrationManager(
            IKrakerApi krakerApi,
            IAgentInfoManager agentInfoManager,
            IAgentInfoProvider agentInfoProvider,
            IAgentIdManager agentIdManager)
        {
            _krakerApi = krakerApi;
            _agentInfoManager = agentInfoManager;
            _agentInfoProvider = agentInfoProvider;
            _agentIdManager = agentIdManager;
        }

        public async Task<(AgentId agentId, AgentInfo agentInfo)> Register()
        {
            var (registrationIsNeeded, agentInfo) = RegistrationIsNeeded();

            if (!registrationIsNeeded)
                return (_agentIdManager.GetCurrent(), agentInfo);
            
            var agentId = await _krakerApi.RegisterAgent();
            
            _agentIdManager.Save(agentId);
            
            _agentInfoManager.Save(agentInfo);

            return (_agentIdManager.GetCurrent(), agentInfo);

        }

        private (bool IsNeeded, AgentInfo ActualAgentInfo) RegistrationIsNeeded()
        {
            var agentId = _agentIdManager.GetFromFile();
            
            var actualAgentInfo = _agentInfoProvider.Get();
            
            if (agentId.Id is null or "")
                return (true, actualAgentInfo);

            var oldAgentInfoOR = _agentInfoManager.GetFromFile();

            if (!oldAgentInfoOR.IsSuccess || oldAgentInfoOR.Result is null)
                return (true, actualAgentInfo);

            var oldAgentInfo = oldAgentInfoOR.Result;

            if (oldAgentInfo.OperationalSystem != actualAgentInfo.OperationalSystem
                || oldAgentInfo.HostName != actualAgentInfo.HostName
                || oldAgentInfo.Ip != actualAgentInfo.Ip)
                return (true, actualAgentInfo);

            return (false,  actualAgentInfo);
        }
    }
}