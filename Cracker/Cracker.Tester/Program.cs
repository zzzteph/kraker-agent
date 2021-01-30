using System;
using System.IO;
using System.Threading;
using Cracker.Base.HashCat;
using Cracker.Base.HttpClient;
using Cracker.Base.Settings;
using FakeItEasy;

namespace CrackerTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var hashCatPath = "C:\\hashcat-6.1.1\\hashcat.exe";
            var hashfilePath = "C:\\hashcat-6.1.1\\example0.hash";
            var wordlistPath = "C:\\hashcat-6.1.1\\example.dict";
            Environment.CurrentDirectory = Path.GetDirectoryName(hashCatPath);
            var command =
                $"--quiet --force --status --status-timer=1 --machine-readable --logfile-disable --restore-disable --outfile-format=2 -m 0 -o ertyui {hashfilePath} {wordlistPath}";
            
            var settings = new HashCatSettings(60, 100, hashCatPath);
            var result = new HashCatCommandExecuter(command, settings, A.Fake<IServerClient>()).Execute(new CancellationToken()).Result;
            Console.WriteLine($"{command} done! ExitCode: {result.HashCatExitCode}, Error: {result.ErrorMessage}");
        }
    }
}