using System;
using System.Collections.Generic;

namespace nLoad
{
    public class ScenarioRunResult
    {
        public Dictionary<string, List<long>> Stats { get; set; }
        public Response Response { get; set; }
        public ScenarioRunResult()
        {
            Stats = new Dictionary<string, List<long>>();
        }
        public static ScenarioRunResult Merge(IEnumerable<ScenarioRunResult> list)
        {
            var result = new ScenarioRunResult();

            foreach (var item in list)
            {
                foreach (var stats in item.Stats)
                {
                    if (result.Stats.ContainsKey(stats.Key)) result.Stats[stats.Key].AddRange(stats.Value);
                    else result.Stats.Add(stats.Key, stats.Value);
                }
            }

            return result;
        }

        public static ScenarioRunResult Fail(Exception e) => new ScenarioRunResult
        {
            Response = Response.Fail(e)
        };
    }
}
