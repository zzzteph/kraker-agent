using System.Text.Json.Serialization;

namespace Cracker.Base.Model
{
    public record AgentInfo
    {
        public AgentInfo(string ip,
            string hostName,
            string os,
            string hashcatVersion,
            string hw) =>
            (Ip, HostName, OperationalSystem, HashcatVersion, HardwareInfo) =
            (ip, hostName, os, hashcatVersion, hw);
        
        [JsonPropertyName("ip")] 
        public string Ip { get; init; }

        [JsonPropertyName("hostname")]
        public string HostName { get; init; }

        [JsonPropertyName("os")] 
        public string OperationalSystem { get; init; }

        [JsonPropertyName("hashcat")]
        public string HashcatVersion { get; init; }

        [JsonPropertyName("hw")] 
        public string HardwareInfo { get; init; }
    }
}