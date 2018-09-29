using System;
using System.IO;

namespace CrackerTester
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("Args: hashcatPath, hashfilePath и wordlistPath");
				return;
			}

			if (!File.Exists(args[0]))
			{
				Console.WriteLine($"Don't exist {args[0]}");
				return;
			}

			Environment.CurrentDirectory = Path.GetDirectoryName(args[0]);

			var command =
				$"--quiet --status --status-timer=1 --machine-readable --logfile-disable --restore-disable --outfile-format=2 -m 0 -o ertyui {args[1]} {args[2]}";
			//var result = new HashCatCommandExecuter(command, args[0]).Execute(new CancellationToken()).Result;
			//Console.WriteLine($"{command} done! ExitCode: {result.HashCatExitCode}, Error: {result.ErrorMessage}");
		}
	}
}