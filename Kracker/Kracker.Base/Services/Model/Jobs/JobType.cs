namespace Kracker.Base.Services.Model.Jobs
{
    public enum JobType
    {
        SpeedStat = 1,
        HashList,
        TemplateBruteforce,
        TemplateWordlist,
        Bruteforce,
        WordList,
        UnrecognizedJob,
        DoNothing
    }
}