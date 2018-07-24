using NLog;

namespace Cracker.Lib.Logging
{
    public static class Log
	{
		private static readonly ILogger logger;
		static Log()
		{
			logger = LogManager.GetCurrentClassLogger();
		}

		public static void Message(string message)
		{
			logger.Log(LogLevel.Info, message);
		}
    }
}
