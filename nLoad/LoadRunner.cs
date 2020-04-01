using ConsoleTables;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nLoad
{
    public static class MyListExtensions
    {
        public static double Mean(this List<long> values)
        {
            return values.Count == 0 ? 0 : values.Mean(0, values.Count);
        }

        public static double Mean(this List<long> values, int start, int end)
        {
            double s = 0;

            for (int i = start; i < end; i++)
            {
                s += values[i];
            }

            return s / (end - start);
        }

        public static double Variance(this List<long> values)
        {
            return values.Variance(values.Mean(), 0, values.Count);
        }

        public static double Variance(this List<long> values, double mean)
        {
            return values.Variance(mean, 0, values.Count);
        }

        public static double Variance(this List<long> values, double mean, int start, int end)
        {
            double variance = 0;

            for (int i = start; i < end; i++)
            {
                variance += Math.Pow((values[i] - mean), 2);
            }

            int n = end - start;
            if (start > 0) n -= 1;

            return variance / (n);
        }

        public static double StandardDeviation(this List<long> values)
        {
            return values.Count == 0 ? 0 : values.StandardDeviation(0, values.Count);
        }

        public static double StandardDeviation(this List<long> values, int start, int end)
        {
            double mean = values.Mean(start, end);
            double variance = values.Variance(mean, start, end);

            return Math.Sqrt(variance);
        }
    }

    public class LoadRunner<T>
    {
        private class Statistics
        {
            public string Name { get; set; }
            public double Mean { get; set; }
            public long Max { get; set; }
            public long Min { get; set; }
            public double Stdev { get; set; }
            public long ExpectCalls { get; set; }
            public long SuccessfullCalss { get; set; }

            public override string ToString()
            {
                return $"{Name}: Mean={Mean}, Max={Max}, Min={Min}, Stdev={Stdev}, {SuccessfullCalss}/{ExpectCalls}";
            }
        }

        private CancellationTokenSource _cts;
        public ScenarioBuilder<T> ScenarioBuilder { get; set; }

        public static LoadRunner<T> RegisterScenarios(Scenario scenario) => new LoadRunner<T>
        {
            ScenarioBuilder = ScenarioBuilder<T>.BuildScenario(scenario)
        };

        public Task RunInConsole(bool saveToFile = true)
        {
            _cts = ScenarioBuilder.ScenarioToRun.DurationLimit.HasValue ? new CancellationTokenSource(ScenarioBuilder.ScenarioToRun.DurationLimit.Value) : new CancellationTokenSource();
            return Task<ScenarioRunResult>.Factory.StartNew(() =>
            {
                try
                {
                    var options = new ProgressBarOptions
                    {
                        ForegroundColor = ConsoleColor.Yellow,
                        BackgroundColor = ConsoleColor.DarkYellow,
                        ProgressCharacter = '─'
                    };
                    using (var pbar = new ProgressBar(1, "Load test progress", options))
                    {
                        var result = ScenarioBuilder.BuildAndRun(_cts.Token, pbar);
                        return result;
                    }
                }
                catch (Exception e)
                {
                    return ScenarioRunResult.Fail(e);
                }
            }).ContinueWith((task) =>
            {
                var result = task.Result;
                var stats = CalculateStatistics(result);
                PrintToConsole(stats);
                if (saveToFile) PrintToFile(stats);
            });
        }

        private List<Statistics> CalculateStatistics(ScenarioRunResult scenarioResult)
        {
            var listResult = new List<Statistics>();

            foreach (var result in scenarioResult.Stats)
            {
                listResult.Add(new Statistics
                {
                    Name = result.Key,
                    ExpectCalls = ScenarioBuilder.ScenarioToRun.ConcurrentCopies * ScenarioBuilder.ScenarioToRun.Repeat,
                    SuccessfullCalss = result.Value.Count,
                    Mean = result.Value.Mean(),
                    Max = result.Value.Max(),
                    Min = result.Value.Min(),
                    Stdev = result.Value.StandardDeviation()
                });
            }

            return listResult;
        }

        private void PrintToConsole(List<Statistics> stats)
        {
            Console.WriteLine();
            ConsoleTable
                .From(stats)
                .Configure(o => o.NumberAlignment = Alignment.Right)
                .Write(Format.Alternative);
        }

        private void PrintToFile(List<Statistics> stats)
        {
            var sb = new StringBuilder();
            stats.ForEach((s) => sb.AppendLine(s.ToString()));
            var fileName = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".txt";
            File.WriteAllText(fileName, sb.ToString());
        }
    }
}
