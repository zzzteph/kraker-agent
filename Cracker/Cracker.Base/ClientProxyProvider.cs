using Cracker.Lib.HttpClient;
using Cracker.Lib.Logging;

namespace Cracker.Lib
{
	public static class ClientProxyProvider
	{
		public static readonly ClientProxy Client =
			new ClientProxy(err => Log.Message("ошибка при обращении к серверу: " + err));
	}
}
