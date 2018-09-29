using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cracker.Base.HashCat;
using Cracker.Base.Logging;
using Cracker.Base.Model;
using Newtonsoft.Json.Linq;

namespace Cracker.Base.Settings
{
	public class AgentInfoManager
	{
		private readonly string agentInfoFilePath;

		public AgentInfoManager(string currentDirectory) =>
			agentInfoFilePath = Path.Combine(currentDirectory, "agentInfo.json");
		

		public AgentInfo Build(Config config)
		{
			Environment.CurrentDirectory = Path.GetDirectoryName(config.HashCatPath);
			var hostName = Dns.GetHostName();
			var hw = new HashCatCommandExecuter($"-I", config).Execute(new CancellationToken(), true).Result.Output;
			return new AgentInfo
			{
				HostName = hostName,
				Ip = Dns.GetHostAddresses(hostName)
						.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
						?.ToString(),
				HashcatVersion = new HashCatCommandExecuter($"-V", config).Execute(new CancellationToken()).Result.Output[0],
				HW = string.Join(Environment.NewLine, hw),
				OperationalSystem =  Environment.OSVersion.VersionString
			};
		}

		public OperationResult<AgentInfo> GetFromFile()
		{
			if (!File.Exists(agentInfoFilePath))
				return null;
			try
			{
				var agentInfo = JObject.Parse(File.ReadAllText(agentInfoFilePath)).ToObject<AgentInfo>();
				return OperationResult<AgentInfo>.Success(agentInfo);
			}
			catch (Exception e)
			{
				Log.Error(e);
				return OperationResult<AgentInfo>.Fail(
					"Файл с информацией об агенте существует но его не удалось получить");
			}
		}


		public OperationResult Save(AgentInfo agentInfo)
		{
			try
			{
				File.WriteAllText(agentInfoFilePath, JObject.FromObject(agentInfo).ToString());
				return OperationResult.Success;
			}
			catch (Exception e)
			{
				Log.Error(e);
				return OperationResult.Fail("Не удалось сохранить файл с информацией об агенте");
			}
		}
	}
}