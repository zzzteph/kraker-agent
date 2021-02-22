using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using Cracker.Base.HashCat;
using Cracker.Base.Logging;
using Cracker.Base.Model;
using Cracker.Base.Services;

namespace Cracker.Base.Settings
{
    public class AgentInfoManager
    {
        private readonly string agentInfoFilePath;

        public AgentInfoManager(string currentDirectory)
        {
            agentInfoFilePath = Path.Combine(currentDirectory, "agentInfo.json");
        }
        
        public AgentInfo Build(HashCatSettings config, IKrakerApi krakerApi)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(config.Path);
            var hostName = Dns.GetHostName();
            var hw = new HashCatCommandExecuter(PrepareJobResult.FromArguments("-I"), config, krakerApi).Execute(new CancellationToken(), true).Result.Output;
            return new AgentInfo
            {
                HostName = hostName,
                Ip = Dns.GetHostAddresses(hostName)
                    .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                    ?.ToString(),
                HashcatVersion = new HashCatCommandExecuter(PrepareJobResult.FromArguments("-V"), config, krakerApi).Execute(new CancellationToken()).Result
                    .Output[0],
                HW = string.Join(Environment.NewLine, hw),
                OperationalSystem = Environment.OSVersion.VersionString
            };
        }

        public OperationResult<AgentInfo> GetFromFile()
        {
            if (!File.Exists(agentInfoFilePath))
                return OperationResult<AgentInfo>.Success(null);;
            try
            {
                var agentInfo = JsonSerializer
                    .Deserialize<AgentInfo>(File.ReadAllText(agentInfoFilePath));
                
                return OperationResult<AgentInfo>.Success(agentInfo);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return OperationResult<AgentInfo>.Fail(
                    "The agent info file exists, but I couldn't read it");
            }
        }


        public OperationResult Save(AgentInfo agentInfo)
        {
            try
            {
                File.WriteAllText(agentInfoFilePath,
                    JsonSerializer.Serialize(agentInfo));
                return OperationResult.Success;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return OperationResult.Fail("Fail during save the agent info file");
            }
        }
    }
}