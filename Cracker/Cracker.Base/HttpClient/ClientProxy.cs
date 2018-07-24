using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Cracker.Lib.AgentSettings;
using Cracker.Lib.HashCat;
using Cracker.Lib.HttpClient.Data;
using Cracker.Lib.Logging;
using Newtonsoft.Json.Linq;

namespace Cracker.Lib.HttpClient
{
    public class ClientProxy
	{
		private static readonly System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
		private readonly Action<string> errorHandler;

		public ClientProxy(Action<string> errorHandler)
		{
			httpClient.BaseAddress = new Uri(SettingsProvider.CurrentSettings.Config.ServerUrl);
			this.errorHandler = errorHandler;
		}

		public async Task<Job> GetJob() =>
		 await GetAsync<Job>($"api/task/get/{SettingsProvider.CurrentSettings.Config.RegistrationKey}");

		public async Task<ServerFile> GetHashFile(string hashId) =>
			await GetAsync<ServerFile>($"api/hash/get/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/{hashId}");

		public async Task<ServerFile> GetPotFile(string hashId) => 
			await GetAsync<ServerFile>($"api/pot/get/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/{hashId}");

		public async Task SendRegistrationKey() =>
			await GetAsync<object>($"api/agent/{SettingsProvider.CurrentSettings.Config.RegistrationKey}");

		public async Task SendAgentInfo() =>
			await PostAsync<AgentInfo>($"api/agent/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/info", () => SettingsProvider.CurrentSettings.Config.AgentInfo);
		
		public async Task SendAgentInventory() =>
			await PostAsync<IList<FileDescription>>($"api/agent/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/inventory", AgentInventoryProvider.Get);

		public async Task SendAgentSpeedStats(params SpeedStat[] speedStats) =>
			await PostAsync<object>($"api/agent/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/speedstats", () => speedStats);

		public async Task<Job> Heartbeat() =>
			await GetAsync<Job>($"api/agent/{SettingsProvider.CurrentSettings.Config.RegistrationKey}");

		public async Task SendJobEnd(object result, string jobId)
		{
			await PostAsync<object>($"api/job/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/{jobId}/end",() => result);
		}
		public async Task SendJobStart(string jobId) =>
			await GetAsync<object>($"api/job/{SettingsProvider.CurrentSettings.Config.RegistrationKey}/{jobId}/start");

		public async Task<T> PostAsync<T>(string requestUri, Func<object> valueProvider)
		{
			try
			{
				var value = valueProvider();
				var valueAsString = value.GetType().IsArray ? JArray.FromObject(value).ToString() : JObject.FromObject(value).ToString();
				
				Log.Message($"POST запрос {requestUri}; Body: {valueAsString}");
				var response = await httpClient.PostAsJsonAsync(requestUri, value);

				if (!response.IsSuccessStatusCode)
				{
					var err = await response.Content.ReadAsStringAsync();
					errorHandler(err);
					return default(T);
				}

				return await response.Content.ReadAsAsync<T>();
			}
			catch (Exception e)
			{
				errorHandler(e.ToString());
				return default(T);
			}
		}

		private async Task<T> GetAsync<T>(string requestUri)
		{
			try
			{
				Log.Message($"GET запрос {requestUri}");
				var response = await httpClient.GetAsync(requestUri);

				return await response.Content.ReadAsAsync<T>();
			}
			catch (Exception e)
			{
				errorHandler(e.ToString());
				return default(T);
			}
		}
	}
}
