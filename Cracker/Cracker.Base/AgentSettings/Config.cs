namespace Cracker.Base.AgentSettings
{
	public class Config
	{
		public string RegistrationKey { get; set; }
		public AgentInfo AgentInfo { get; set; }
		public string HashCatPath { get; set; }
		public string ServerUrl { get; set; }
		public int? InventoryCheckPeriod { get; set; }
		public int? HearbeatPeriod { get; set; }
		public int? KillHashcatSilencePeriod { get; set; }
		public int? KillHashcatAfterRepeatedStrings { get; set; }
	}
}