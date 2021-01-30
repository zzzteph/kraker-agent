namespace Cracker.Base.Settings
{
    public record Config
    {
        public string RegistrationKey { get; set; }
        public HashCatSettings HashCat { get; set; } 
        public string ServerUrl { get; set; }
        public int? InventoryCheckPeriod { get; set; }
        public int? HearbeatPeriod { get; set; }
    }

    public record HashCatSettings(int? SilencePeriodBeforeKill,
        int? RepeatedStringsBeforeKill,
        string Path);
}