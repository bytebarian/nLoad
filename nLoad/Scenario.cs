using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace nLoad
{
    public class Scenario
    {
        public List<Step> Steps { get; set; }
        public string Name { get; set; }
        public ConnectionPool ConnectionPool { get; set; }
        public TimeSpan? DurationLimit { get; set; }
        public int ConcurrentCopies { get; set; }
        public int Repeat { get; set; }

        public static Scenario CreateScenario(string name, List<Step> steps, ConnectionPool connPool, int concurentCopies = 1, int repeat = 1, TimeSpan? duration = null) => new Scenario
        {
            Name = name,
            Steps = steps,
            ConnectionPool = connPool,
            ConcurrentCopies = concurentCopies,
            Repeat = repeat,
            DurationLimit = duration
        };

        internal static ScenarioRunResult Run(int id, Scenario scenario, CancellationToken token, IProgress<int> progress, int iteration)
        {
            var context = new StepContext();
            context.Id = id;
            var result = new ScenarioRunResult();
            var i = 0;
            foreach (var step in scenario.Steps)
            {
                if (token.IsCancellationRequested) break;

                context.Connection = scenario.ConnectionPool.OpenConnection();

                var watch = Stopwatch.StartNew();
                var response = step.Executor(context);
                watch.Stop();

                if (response.Exception != null || !string.IsNullOrEmpty(response.FailReason))
                {
                    result.Response = response;
                    var msg = response.Exception != null ? response.Exception.Message : response.FailReason;
                    progress?.Report(iteration * scenario.Steps.Count);
                    break;
                }
                else
                {
                    context.Data = response.Data;
                    if (result.Stats.ContainsKey(step.Name)) result.Stats[step.Name].Add(watch.ElapsedMilliseconds);
                    else result.Stats.Add(step.Name, new List<long> { watch.ElapsedMilliseconds });
                    i++;
                    progress?.Report(iteration * i);
                }

                scenario.ConnectionPool.CloseConnection(context.Connection);
            }

            return result;
        }
    }
}
