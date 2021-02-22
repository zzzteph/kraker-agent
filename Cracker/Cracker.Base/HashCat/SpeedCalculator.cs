using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cracker.Base.Logging;

namespace Cracker.Base.HashCat
{
    public static class SpeedCalculator
    {
        public static BigInteger CalculateBenchmark(IReadOnlyList<string> lines)
        {
            return lines.Select(l => l.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries))
                .Where(la => la.Length == 6)
                .Select(la => BigInteger.Parse(la[5])).Aggregate(BigInteger.Zero, (abi, bi) => abi + bi);
        }

        public static double CalculateFact(IReadOnlyList<string> hashcatOut)
        {
            if (hashcatOut == null || hashcatOut.Count == 0)
                return 0d;

            var speeds = new double[hashcatOut.Count];
            for (var j = 0; j < hashcatOut.Count; j++)
                try
                {
                    var outStr = hashcatOut[j];
                    var start = outStr.IndexOf("SPEED	");
                    var end = outStr.IndexOf("EXEC_RUNTIME");
                    var speed = outStr.Substring(start + 6, end - start - 6)
                        .Split(new[] {"	"}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(double.Parse).ToArray();

                    var cs = 0d;
                    for (var i = 0; i < speed.Length / 2; i++)
                        cs += speed[2 * i] * 1000 / speed[2 * i + 1];

                    speeds[j] = cs;
                }
                catch (Exception e)
                {
                    Log.Message($"[Скорость] Не удалось рассчитать для выдачи hashcat: {hashcatOut[j]}. {e}");
                }

            return speeds.Average();
        }
    }
}