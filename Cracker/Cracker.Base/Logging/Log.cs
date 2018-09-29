using System;
using NLog;

namespace Cracker.Base.Logging
{
    public static class Log
	{
		private static readonly ILogger logger;
		static Log() => logger = LogManager.GetCurrentClassLogger();

		public static void Message(string message) => logger.Log(LogLevel.Info, message);
		public static void Error(Exception exception) => Error(exception.ToString());
		public static void Error(string message) => logger.Log(LogLevel.Error, message);
    }
}
