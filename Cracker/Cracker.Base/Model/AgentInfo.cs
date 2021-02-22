using System.Text.Json.Serialization;

namespace Cracker.Base.Model
{
    public class AgentInfo
    {
        [JsonPropertyName("ip")] public string Ip { get; set; }

        [JsonPropertyName("hostname")]
        public string HostName { get; set; }

        [JsonPropertyName("os")] 
        public string OperationalSystem { get; set; }

        [JsonPropertyName("hashcat")]
        public string HashcatVersion { get; set; }

        [JsonPropertyName("hw")] 
        public string HW { get; set; }
    }
}