using System.IO;
using Cracker.Base.Model;
using Cracker.Base.Model.Jobs;
using Cracker.Base.Settings;

namespace Cracker.Base.HashCat
{
    public class ArgumentsBuilder
    {
        //todo вынести в конфиг. Можно прикрепить и форс сюда же
        private const string Options =
            "--quiet --status --status-timer=1 --machine-readable --logfile-disable --restore-disable --outfile-format=2";

        private readonly WorkedDirectories workedDirectories;

        public ArgumentsBuilder(WorkedDirectories workedDirectories)
        {
            this.workedDirectories = workedDirectories;
        }

        public string BuildArguments(AbstractJob job, TempFilePaths paths) =>
            job switch
            {
                TemplateMaskJob tmj => $"{Options} -a 3 "
                                       + (tmj.Charset1 is null ? string.Empty : $"-1 {tmj.Charset1} ")
                                       + (tmj.Charset2 is null ? string.Empty : $"-2 {tmj.Charset2} ")
                                       + (tmj.Charset3 is null ? string.Empty : $"-3 {tmj.Charset3} ")
                                       + (tmj.Charset4 is null ? string.Empty : $"-4 {tmj.Charset4} ")
                                       + tmj.Mask,

                TemplateWordListJob twl => $"{Options} {BuildRule(twl.Rule)}"
                                           + $" \"{Path.Combine(workedDirectories.WordlistPath, twl.Wordlist)}\"",

                HashListJob hlj => $"{Options} -m {hlj.HashTypeId} {BuildFilePaths(paths)}",

                BruteforceJob bfj => $"{Options} --skip={bfj.Skip} --limit={bfj.Limit} -m {bfj.HashListTypeId} "
                                     + $" --outfile=\"{paths.OutputFile}\" "
                                     + $"{BuildFilePaths(paths)} {bfj.Mask}",

                WordListJob wlj => $"{Options} --skip={wlj.Skip} --limit={wlj.Limit} -m {wlj.HashTypeId} "
                                   + BuildRule(wlj.Rule)
                                   + $" --outfile=\"{paths.OutputFile}\" " +
                                   $"{BuildFilePaths(paths)} \"{Path.Combine(workedDirectories.WordlistPath, wlj.Wordlist)}\"",

                _ => null
            };


        private string BuildRule(string? rule) =>
            string.IsNullOrEmpty(rule)
                ? string.Empty
                : $"-r \"{Path.Combine(workedDirectories.RulesPath, rule)}\"";

        private string BuildFilePaths(TempFilePaths paths) =>
            paths.PotFile == null
                ? $" \"{paths.HashFile}\" "
                : $"--potfile-path=\"{paths.PotFile}\" \"{paths.HashFile}\" ";
    }
}