using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cracker.Base.Logging;
using Newtonsoft.Json.Linq;

namespace Cracker.Base.HttpClient
{
    public class BaseClient
    {
        private static BaseClient baseClient;
        private static readonly object sync = new();

        private readonly System.Net.Http.HttpClient httpClient;

        private BaseClient(string baseAddress)
        {
            httpClient = HttpClientFactory.Create();
            httpClient.BaseAddress = new Uri(baseAddress);
        }

        public static BaseClient Instance(string serverUrl)
        {
            if (baseClient == null)
                lock (sync)
                {
                    if (baseClient == null) return baseClient = new BaseClient(serverUrl);
                }

            return baseClient;
        }


        public async Task<T> PostAsync<T>(string requestUri, Func<object> valueProvider)
        {
            try
            {
                var value = valueProvider();
                var valueAsString = value.GetType().IsArray
                    ? JArray.FromObject(value).ToString()
                    : JObject.FromObject(value).ToString();

                Log.Message($"POST запрос {requestUri}; Body: {valueAsString}");
                var response = await httpClient.PostAsJsonAsync(requestUri, value);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Log.Error(error);
                    return default;
                }

                return await response.Content.ReadAsAsync<T>();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return default;
            }
        }

        public async Task<T> GetAsync<T>(string requestUri)
        {
            try
            {
                Log.Message($"GET запрос {requestUri}");
                var response = await httpClient.GetAsync(requestUri);

                return await response.Content.ReadAsAsync<T>();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return default;
            }
        }
    }
}