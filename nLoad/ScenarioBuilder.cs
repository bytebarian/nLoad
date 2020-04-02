using ShellProgressBar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace nLoad
{
    public class ScenarioBuilder
    {
        public Scenario ScenarioToRun { get; set; }
        private ConcurrentBag<ScenarioRunResult> resultBug = new ConcurrentBag<ScenarioRunResult>();

        public ScenarioBuilder(Scenario scenario)
        {
            ScenarioToRun = scenario;
        }

        public static ScenarioBuilder BuildScenario(Scenario scenario) => new ScenarioBuilder(scenario);

        public ScenarioRunResult BuildAndRun(CancellationToken ct, ProgressBar pbar)
        {
            var tasks = new List<Task<ScenarioRunResult>>();

            Func<object, ScenarioRunResult> action = (object index) =>
            {
                var resultList = new List<ScenarioRunResult>();
                var childOptions = new ProgressBarOptions
                {
                    ForegroundColor = ConsoleColor.Green,
                    BackgroundColor = ConsoleColor.DarkGreen,
                    ProgressCharacter = '─'
                };
                var pbarChild = pbar?.Spawn(ScenarioToRun.Repeat * ScenarioToRun.Steps.Count, $"concurent run ({index})", childOptions);
                var progress = pbarChild?.AsProgress<int>();
                for (int ind = (int)index, i = 0; i < ScenarioToRun.Repeat; i++, ind++)
                {
                    if (ct.IsCancellationRequested) break;

                    resultList.Add(Scenario.Run(ind, ScenarioToRun, ct, progress, i));
                }
                if (pbarChild != null) pbarChild.Dispose();
                var result = ScenarioRunResult.Merge(resultList);
                resultBug.Add(result);
                return result;
            };

            for (int i = 0; i < (ScenarioToRun.ConcurrentCopies * ScenarioToRun.Repeat); i += ScenarioToRun.Repeat)
            {
                int index = i;
                tasks.Add(Task<ScenarioRunResult>.Factory.StartNew(action, index, ct));
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
                pbar?.AsProgress<float>().Report(1);
                return ScenarioRunResult.Merge(resultBug.ToArray());
            }
            catch (AggregateException e)
            {
                return ScenarioRunResult.Fail(e);
            }
        }
    }
}
