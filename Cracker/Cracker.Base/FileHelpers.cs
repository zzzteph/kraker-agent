using System;
using System.IO;

namespace Cracker.Base
{
    public static class FileHelpers
    {
		public static void SoftDelete(string path, string semanticName)
		{
			try
			{
				File.Delete(path);
			}
			catch (Exception e)
			{
				Logging.Log.Message($"Не удаляется {semanticName}: {path}, {e}");
			}
		}
	}
}
