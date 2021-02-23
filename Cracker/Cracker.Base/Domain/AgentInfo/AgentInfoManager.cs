using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base.Domain.HashCat;
using Cracker.Base.Model;
using Cracker.Base.Settings;
using Serilog;
using static Cracker.Base.Model.Constants;

namespace Cracker.Base.Domain.AgentInfo
{
    public interface IAgentInfoManager
    {
        Task<Model.AgentInfo> Build();
        OperationResult<Model.AgentInfo> GetFromFile();
        OperationResult Save(Model.AgentInfo agentInfo);
    }

    public class AgentInfoManager : IAgentInfoManager
    {
        private readonly string _agentInfoFilePath;
        private readonly ILogger _logger;
        private readonly HashCatSettings _settings;
        private readonly IWorkingDirectoryProvider _workingDirectoryProvider;

        public AgentInfoManager(ILogger logger, 
            Config config,
            AppFolder appFolder, 
            IWorkingDirectoryProvider workingDirectoryProvider)
        {
            _logger = logger;
            _workingDirectoryProvider = workingDirectoryProvider;
            _agentInfoFilePath = Path.Combine(appFolder.Value, ArtefactsFolder, AgentInfoFile);
            _settings = config.HashCat;
        }

        public OperationResult<Model.AgentInfo> GetFromFile()
        {
            if (!File.Exists(_agentInfoFilePath))
                return OperationResult<Model.AgentInfo>.Success(null);
            try
            {
                var agentInfo = JsonSerializer
                    .Deserialize<Model.AgentInfo>(File.ReadAllText(_agentInfoFilePath));

                return OperationResult<Model.AgentInfo>.Success(agentInfo);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Can't read the agent info file");

                return OperationResult<Model.AgentInfo>.Fail(
                    "The agent info file exists, but can't read it");
            }
        }

        public OperationResult Save(Model.AgentInfo agentInfo)
        {
            try
            {
                File.WriteAllText(_agentInfoFilePath,
                    JsonSerializer.Serialize(agentInfo));
                return OperationResult.Success;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Fail during save the agent info fil");
                return OperationResult.Fail("Fail during save the agent info file");
            }
        }

        public async Task<Model.AgentInfo> Build()
        {
            var hostName = Dns.GetHostName();

            var workingDirectory = _workingDirectoryProvider.Get();

            var hw = new HashCatCommandExecuter(PrepareJobResult.FromArguments("-I"), _settings, _logger, workingDirectory)
                .Execute(new CancellationToken(), true).Result.Output;

            var ip = Dns.GetHostAddresses(hostName)
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                ?.ToString();

            var hashcatVersion =
                await new HashCatCommandExecuter(PrepareJobResult.FromArguments("-V"), _settings, _logger, workingDirectory)
                    .Execute(new CancellationToken());

            var os = Environment.OSVersion.VersionString;

            return new Model.AgentInfo
                {
                    Ip = ip,
                    HostName = hostName,
                    OperationalSystem = os,
                    HashcatVersion = hashcatVersion.Output[0],
                    HardwareInfo = string.Join(Environment.NewLine, hw)
                };
        }
    }
}