using Newtonsoft.Json;

namespace Cracker.Base.Settings
{
    public class AgentInfo
    {
        [JsonProperty(PropertyName = "ip")] public string Ip { get; set; }


        [JsonProperty(PropertyName = "hostname")]
        public string HostName { get; set; }

        [JsonProperty(PropertyName = "os")] public string OperationalSystem { get; set; }

        [JsonProperty(PropertyName = "hashcat")]
        public string HashcatVersion { get; set; }

        [JsonProperty(PropertyName = "hw")] public string HW { get; set; }
    }
}