namespace Cracker.Base.Settings
{
    public record Config(string AgentId,
        HashCatSettings HashCat,
        string ServerUrl,
        int? InventoryCheckPeriod,
        int? HearbeatPeriod);

    public record HashCatSettings(int? SilencePeriodBeforeKill,
        int? RepeatedStringsBeforeKill,
        string Path,
        string Options);
}