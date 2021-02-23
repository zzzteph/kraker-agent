using System.IO;
using Cracker.Base.Model;
using Cracker.Base.Settings;

namespace Cracker.Base.Domain.AgentId
{
    public interface IAgentIdManager
    {
        Model.AgentId GetCurrent();
        Model.AgentId GetFromFile();
        void Save(Model.AgentId agentId);
    }

    public class AgentIdManager : IAgentIdManager
    {
        private readonly string _agentIdFilePath;
        private Model.AgentId? _current;

        public AgentIdManager(AppFolder appFolder)
        {
            _agentIdFilePath = Path.Combine(appFolder.Value, Constants.ArtefactsFolder, Constants.AgentIdFile);
            _current = null;
        }

        public Model.AgentId GetCurrent()
        {
            return _current;
        }

        public Model.AgentId GetFromFile()
        {
            var agentId = File.Exists(_agentIdFilePath) 
                ? new Model.AgentId(File.ReadAllText(_agentIdFilePath))
                : new Model.AgentId(null);
            
            _current = agentId;
            return agentId;
        }
            

        public void Save(Model.AgentId agentId)
        {
            if (agentId is null || agentId.Id is null)
                File.Delete(_agentIdFilePath);
            
            File.WriteAllText(_agentIdFilePath,agentId.Id);
            _current = new Model.AgentId(agentId.Id);
        }
    }
}