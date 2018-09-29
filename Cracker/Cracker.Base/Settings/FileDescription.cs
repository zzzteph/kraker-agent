using System;
using Newtonsoft.Json;

namespace Cracker.Base.Settings
{
	public class FileDescription
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "size")]
		public long Size { get; set; }

		[JsonProperty(PropertyName = "wordscount")]
		public long LinesCount { get; set; }
		[JsonProperty(PropertyName = "hash")]
		public string Hash { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string FolderName { get; set; }

		[JsonProperty(PropertyName = "lastwritetime")]
		public DateTime LastWriteTime { get; set; }
	}
}