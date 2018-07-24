using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cracker.Base.HashCat;

namespace Cracker.Base.AgentSettings
{
	public static class AgentInfoProvider
	{
		public static AgentInfo Get()
		{
			var hascatPath = SettingsProvider.CurrentSettings.Config.HashCatPath;
			Environment.CurrentDirectory = Path.GetDirectoryName(hascatPath);
			var hostName = Dns.GetHostName();
			var hw = new HashCatCommandExecuter($"-I", hascatPath).Execute(new CancellationToken(), true).Result.Output;
			return new AgentInfo
			{
				HostName = hostName,
				Ip = Dns.GetHostAddresses(hostName)
						.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
						?.ToString(),
				HashcatVersion = new HashCatCommandExecuter($"-V", hascatPath).Execute(new CancellationToken()).Result.Output[0],
				HW = string.Join(Environment.NewLine, hw),
				OperationalSystem =  Environment.OSVersion.VersionString
			};
		}
	}
}