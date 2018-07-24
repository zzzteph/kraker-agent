using System.Numerics;
using Newtonsoft.Json;

namespace Cracker.Lib.HttpClient.Data
{
	public class Command
	{
		public string[] Options { get; set; }
		public BigInteger Skip { get; set; }
		public BigInteger Limit { get; set; }
		public string M { get; set; }

		[JsonProperty(PropertyName = "template_options")]
		public TemplateOptions TemplateOptions { get; set; }

	}
}