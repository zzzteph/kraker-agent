using System;
using System.IO;

namespace Cracker.Lib.HashCat
{
	public static class TempFilePathExtensions
	{
		public static TempFilePaths BuildTempFilePaths(this string directoryPath)
		{
			return new TempFilePaths
			{
				PotFile = BuildTempFilePath(directoryPath),
				HashFile = BuildTempFilePath(directoryPath),
				OutputFile = BuildTempFilePath(directoryPath)
			};
		}

		public static string BuildTempFilePath(this string directoryPath)
		{
			string filePath;
			do
			{
				filePath = Path.Combine(directoryPath, Path.GetRandomFileName());
			} while (File.Exists(filePath));

			return filePath;
		}

		public static void SoftDelete(this string path, string semanticName)
		{
			try
			{
				if (path != null && File.Exists(path))
					File.Delete(path);
			}
			catch (Exception e)
			{
				Logging.Log.Message($"Не удаляется {semanticName}: {path}, {e}");
			}
		}
	}
}