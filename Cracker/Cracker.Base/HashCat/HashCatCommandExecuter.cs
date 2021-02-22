﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cracker.Base.Logging;
using Cracker.Base.Services;
using Cracker.Base.Settings;

namespace Cracker.Base.HashCat
{
    public class HashCatCommandExecuter
    {
        private static readonly Regex _rg = new("STATUS[ \t]*([4,6-8])[ \t]*", RegexOptions.IgnoreCase);
        private readonly ProcessStartInfo _startInfo;
        private readonly int _repeatedStringsLimit;
        private readonly int _silencePeriodLimit;
        private readonly IKrakerApi _krakerApi;
        private readonly object _sync;
        private readonly PrepareJobResult _prepareJobResult;

        public HashCatCommandExecuter(PrepareJobResult prepareJobResult, HashCatSettings config, IKrakerApi krakerApi)
        {
            _prepareJobResult = prepareJobResult;
            _krakerApi = krakerApi;

            _startInfo = new ProcessStartInfo
            {
                FileName = Path.GetFileName(config.Path),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                Arguments = prepareJobResult.HashCatArguments,
                WorkingDirectory = Path.GetDirectoryName(config.Path)
            };

            _sync = new object();

            _silencePeriodLimit = config.SilencePeriodBeforeKill ?? 60;
            _repeatedStringsLimit = config.RepeatedStringsBeforeKill ?? 1000;


            Log.Message($"Build a command for hashcat:{_startInfo.FileName} {_startInfo.Arguments}");
        }

        public async Task<ExecutionResult> Execute(CancellationToken ct, bool waitNullReceiveOutput = false)
        {
            
            Timer workTimer = null;
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
                    StartInfo = _startInfo,
                    EnableRaisingEvents = true
                })
                {
                    var receiveNullData = new TaskCompletionSource<bool>();
                    var silencePeriod = TimeSpan.FromMinutes(_silencePeriodLimit);
                    workTimer = new Timer(o =>
                    {
                        if (taskEnd || wasKill)
                            return;

                        process.Kill();
                        wasKill = true;

                        Log.Message(
                            $"Haven't been any output from hashcat for {_silencePeriodLimit} minutes. Kill the process for job: {_prepareJobResult.Job}");
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

                        if (outputIsTheSame > _repeatedStringsLimit)
                        {
                            Log.Message($"Too long get the same output. Repeats number: {outputIsTheSame}");
                            isSuccessful = false;
                        }

                        if (ct.IsCancellationRequested)
                            isSuccessful = false;

                        if (!_rg.IsMatch(ea.Data) && isSuccessful)
                            return;

                        lock (_sync)
                        {
                            if (wasKill)
                                return;
                            try
                            {
                                Log.Message("Kill hashcat");
                                var proc = s as Process;
                                proc.StandardInput.WriteLineAsync("q");

                                if (!proc.WaitForExit(1500))
                                    process.Kill();

                                wasKill = true;
                            }
                            catch (Exception e)
                            {
                                Log.Message($"Get an exception during killing the process {e}");
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
                    return new ExecutionResult(code,
                        output,
                        errors,
                        _prepareJobResult.Paths,
                        _prepareJobResult.Job,
                        isSuccessful,
                        null
                    );
                }
            }
            catch (Exception e)
            {
                workTimer?.Dispose();

                Log.Message($"Get an exception during hashcat working: {e}");
                return ExecutionResult.FromError(e.ToString());
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