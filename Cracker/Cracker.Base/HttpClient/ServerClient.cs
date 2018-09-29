using System.Collections.Generic;
using System.Threading.Tasks;
using Cracker.Base.HashCat;
using Cracker.Base.HttpClient.Data;
using Cracker.Base.Settings;

namespace Cracker.Base.HttpClient
{
	public class ServerClient
	{
		private readonly BaseClient client;
		private readonly Config config;

		public ServerClient(Config config)
		{
			client = BaseClient.Instance(config.ServerUrl);
			this.config = config;
		}

		public async Task<Job> GetJob() =>
		 await client.GetAsync<Job>($"api/task/get/{config.RegistrationKey}");

		public async Task<ServerFile> GetHashFile(string hashId) =>
			await client.GetAsync<ServerFile>($"api/hash/get/{config.RegistrationKey}/{hashId}");

		public async Task<ServerFile> GetPotFile(string hashId) =>
			await client.GetAsync<ServerFile>($"api/pot/get/{config.RegistrationKey}/{hashId}");

		public async Task SendRegistrationKey() =>
			await client.GetAsync<object>($"api/agent/{config.RegistrationKey}");

		public async Task SendAgentInfo(AgentInfo agentInfo) =>
			await client.PostAsync<AgentInfo>($"api/agent/{config.RegistrationKey}/info", () => agentInfo);

		public async Task SendAgentInventory(FileDescription[] agentInventory) =>
			await client.PostAsync<IList<FileDescription>>($"api/agent/{config.RegistrationKey}/inventory",
				() => agentInventory);

		public async Task SendAgentSpeedStats(params SpeedStat[] speedStats) =>
			await client.PostAsync<object>($"api/agent/{config.RegistrationKey}/speedstats", () => speedStats);

		public async Task<Job> Heartbeat() =>
			await client.GetAsync<Job>($"api/agent/{config.RegistrationKey}");

		public async Task SendJobEnd(object result, string jobId)
		{
			await client.PostAsync<object>($"api/job/{config.RegistrationKey}/{jobId}/end", () => result);
		}
		public async Task SendJobStart(string jobId) =>
			await client.GetAsync<object>($"api/job/{config.RegistrationKey}/{jobId}/start");

		public BaseClient Client => client;

	}
}
