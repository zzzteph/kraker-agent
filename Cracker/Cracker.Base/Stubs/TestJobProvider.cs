using Cracker.Lib.HttpClient.Data;
using Newtonsoft.Json.Linq;

namespace Cracker.Lib.Stubs
{
	public static class TestJobProvider
	{
		public static Job Get()
		{
			return JObject.Parse(@"{
    ""command"": {
		""options"": [
		""--quiet"",
		""--status"",
		""--status-timer=1"",
		""--machine-readable"",
		""--logfile-disable"",
		""--restore-disable""
		],
		""skip"": 0,
		""limit"": 0,
		""m"": 0,
		""type"": ""dictionary"",
		""wordlist"": {
			""wordlist"": ""10_million_password_list_top_1000000.txt"",
			""rule"": ""toggles4.rule""
		}
	},
	""potfile"": """",
	""hashfile"": ""MDA4MGEwODVhZDc1NGYyMmI3ODgyN2U3ZDY4NmI1ZDg="",
	""taskid"": ""1""
}").ToObject<Job>();

		}
	}
}