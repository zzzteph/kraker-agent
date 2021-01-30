using System;
using System.IO;
using Cracker.Base.Logging;

namespace Cracker.Base.HashCat
{
    public static class TempFilePathExtensions
    {
        public static TempFilePaths BuildTempFilePaths(this string directoryPath)
        {
            return new()
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
                Log.Message($"Не удаляется {semanticName}: {path}, {e}");
            }
        }
    }
}