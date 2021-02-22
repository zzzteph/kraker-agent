using System;
using System.Text.Json.Serialization;

namespace Cracker.Base.Model
{
    public class FileDescription
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("size")] public long Size { get; set; }

        [JsonPropertyName("count")]
        public long LinesCount { get; set; }

        [JsonPropertyName("checksum")] 
        public string Сhecksum { get; set; }

        [JsonPropertyName("type")] 
        public string FolderName { get; set; }

        [JsonPropertyName("lastwritetime")]
        public DateTime LastWriteTime { get; set; }
    }
}