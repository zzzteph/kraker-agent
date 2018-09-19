using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Cracker.Base.AgentSettings
{
	public static class FileDescriptionBuilder
	{
		public static FileDescription Build(string filePath)
		{
			var fileInfo = new FileInfo(filePath);
			var fd = new FileDescription
			{
				Name = fileInfo.Name,
				Size = fileInfo.Length,
				FolderName = fileInfo.Directory?.Name,
				LastWriteTime = fileInfo.LastWriteTime
			};

			using (var md5 = MD5.Create())
			using (var reader = fileInfo.OpenRead())
			{
				var hash = md5.ComputeHash(reader);
				fd.Hash = string.Concat(hash.Select(b => b.ToString("X2")));
			}

			using (var reader = fileInfo.OpenText())
			{
				while (reader.ReadLine() != null)
					fd.LinesCount++;
			}

			return fd;
		}
	}
}