using System.Collections.Generic;
using System.Threading.Tasks;
using Cracker.Base.HashCat;
using Cracker.Base.HttpClient.Data;
using Cracker.Base.Settings;

namespace Cracker.Base.HttpClient
{
    public interface IServerClient
    {
        Task<Job> GetJob();
        Task<ServerFile> GetHashFile(string hashId);
        Task<ServerFile> GetPotFile(string hashId);
        Task SendRegistrationKey();
        Task SendAgentInfo(AgentInfo agentInfo);
        Task SendAgentInventory(FileDescription[] agentInventory);
        Task SendAgentSpeedStats(params SpeedStat[] speedStats);
        Task<Job> Heartbeat();
        Task SendJobEnd(object result, string jobId);
        Task SendJobStart(string jobId);
    }

    public class ServerClient : IServerClient
    {
        private readonly Config config;

        public ServerClient(Config config)
        {
            Client = BaseClient.Instance(config.ServerUrl);
            this.config = config;
        }

        public BaseClient Client { get; }

        public async Task<Job> GetJob()
        {
            return await Client.GetAsync<Job>($"api/task/get/{config.RegistrationKey}");
        }

        public async Task<ServerFile> GetHashFile(string hashId)
        {
            return await Client.GetAsync<ServerFile>($"api/hash/get/{config.RegistrationKey}/{hashId}");
        }

        public async Task<ServerFile> GetPotFile(string hashId)
        {
            return await Client.GetAsync<ServerFile>($"api/pot/get/{config.RegistrationKey}/{hashId}");
        }

        public async Task SendRegistrationKey()
        {
            await Client.GetAsync<object>($"api/agent/{config.RegistrationKey}");
        }

        public async Task SendAgentInfo(AgentInfo agentInfo)
        {
            await Client.PostAsync<AgentInfo>($"api/agent/{config.RegistrationKey}/info", () => agentInfo);
        }

        public async Task SendAgentInventory(FileDescription[] agentInventory)
        {
            await Client.PostAsync<IList<FileDescription>>($"api/agent/{config.RegistrationKey}/inventory",
                () => agentInventory);
        }

        public async Task SendAgentSpeedStats(params SpeedStat[] speedStats)
        {
            await Client.PostAsync<object>($"api/agent/{config.RegistrationKey}/speedstats", () => speedStats);
        }

        public async Task<Job> Heartbeat()
        {
            return await Client.GetAsync<Job>($"api/agent/{config.RegistrationKey}");
        }

        public async Task SendJobEnd(object result, string jobId)
        {
            await Client.PostAsync<object>($"api/job/{config.RegistrationKey}/{jobId}/end", () => result);
        }

        public async Task SendJobStart(string jobId)
        {
            await Client.GetAsync<object>($"api/job/{config.RegistrationKey}/{jobId}/start");
        }
    }
}