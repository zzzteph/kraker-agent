using Cracker.Base.HttpClient;
using Cracker.Base.Logging;

namespace Cracker.Base
{
	public static class ClientProxyProvider
	{
		public static readonly ClientProxy Client =
			new ClientProxy(err => Log.Message("ошибка при обращении к серверу: " + err));
	}
}
