using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base.HttpClient;
using Cracker.Base.Logging;
using Cracker.Base.Settings;

namespace Cracker.Base.HashCat
{
    public class HashCatCommandExecuter
    {
        private static readonly Regex rg = new("STATUS[ \t]*([4,6-8])[ \t]*", RegexOptions.IgnoreCase);
        private static Timer workTimer;
        private readonly ProcessStartInfo hashcatStartInfo;
        private readonly int killHashcatAfterRepeatedStrings;
        private readonly int killHashcatSilencePeriod;
        private readonly IServerClient serverClient;
        private readonly object sync;

        public HashCatCommandExecuter(string arguments, HashCatSettings config, IServerClient serverClient)
        {
            hashcatStartInfo = new ProcessStartInfo
            {
                FileName = Path.GetFileName(config.Path),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(config.Path)
            };
            sync = new object();
            killHashcatSilencePeriod = config.SilencePeriodBeforeKill ?? 60;
            killHashcatAfterRepeatedStrings = config.RepeatedStringsBeforeKill ?? 1000;
            Log.Message($"Команда для hashCat:{hashcatStartInfo.FileName} {hashcatStartInfo.Arguments}");
            this.serverClient = serverClient;
        }

        public async Task<ExecutionResult> Execute(CancellationToken ct, bool waitNullReceiveOutput = false,
            string jobId = "")
        {
            try
            {
                var output = new List<string>();
                var errors = new List<string>();
                var outputIsTheSame = 0;
                var isSuccessful = true;
                var wasKill = false;
                var taskEnd = false;

                using (var process = new Process
                {
                    StartInfo = hashcatStartInfo,
                    EnableRaisingEvents = true
                })
                {
                    var receiveNullData = new TaskCompletionSource<bool>();
                    var silencePeriod = TimeSpan.FromMinutes(killHashcatSilencePeriod);
                    workTimer = new Timer(o =>
                    {
                        if (taskEnd || wasKill)
                            return;

                        process.Kill();
                        wasKill = true;
                        serverClient
                            .SendJobEnd(
                                new
                                {
                                    error =
                                        $"Не было output от hashcat в течении {killHashcatSilencePeriod} минут. Сворачиваем лавочку"
                                }, jobId)
                            .ConfigureAwait(false);
                    }, null, silencePeriod, TimeSpan.FromMilliseconds(-1));


                    process.OutputDataReceived += (s, ea) =>
                    {
                        if (!taskEnd)
                            workTimer.Change(silencePeriod, TimeSpan.FromMilliseconds(-1));

                        if (ea?.Data == null)
                        {
                            receiveNullData.SetResult(true);
                            return;
                        }

                        outputIsTheSame = ea.Data == output.LastOrDefault() ? outputIsTheSame + 1 : 0;
                        output.Add(ea.Data);
                        Log.Message($"Hashcat out: {ea.Data}");

                        if (outputIsTheSame > killHashcatAfterRepeatedStrings)
                        {
                            Log.Message($"Долго получаем одинаковый вывод. Количество повторов - {outputIsTheSame}");
                            isSuccessful = false;
                        }

                        ;

                        if (ct.IsCancellationRequested)
                            isSuccessful = false;

                        if (!rg.IsMatch(ea.Data) && isSuccessful)
                            return;

                        lock (sync)
                        {
                            if (wasKill)
                                return;
                            try
                            {
                                Log.Message("Убиваем hashcat");
                                var proc = s as Process;
                                proc.StandardInput.WriteLineAsync("q");

                                if (!proc.WaitForExit(1500))
                                    process.Kill();

                                wasKill = true;
                            }
                            catch (Exception e)
                            {
                                Log.Message($"При умерщвлении процесса получили исключение {e}");
                            }
                        }
                    };

                    process.ErrorDataReceived += (s, ea) =>
                    {
                        if (ea?.Data == null)
                            return;

                        errors.Add(ea.Data);
                        Log.Message($"Hashcat err: {ea.Data}");
                    };

                    var code = await RunProcessAsync(process);
                    taskEnd = true;
                    if (waitNullReceiveOutput)
                        await receiveNullData.Task;
                    workTimer.Dispose();
                    return new ExecutionResult
                    {
                        HashCatExitCode = code,
                        Output = output,
                        Errors = errors,
                        IsSuccessful = isSuccessful
                    };
                }
            }
            catch (Exception e)
            {
                workTimer?.Dispose();

                Log.Message($"Исключение при работе с Hashcat: {e}");
                return new ExecutionResult
                {
                    ErrorMessage = e.ToString()
                };
            }
        }

        private Task<int> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.Exited += (s, ea) =>
            {
                try
                {
                    var proc = s as Process;
                    tcs.SetResult(proc.ExitCode);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Ошибка при обработке завершения процесса: {e}");
                    tcs.TrySetResult(-1);
                }
            };

            if (!process.Start()) throw new InvalidOperationException("Could not start process: " + process);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
    }
}