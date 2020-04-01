using System;

namespace nLoad
{
    public class Step
    {
        public string Name { get; set; }
        public Func<StepContext, Response> Executor { get; set; }

        public static Step Create(string name, Func<StepContext, Response> executor) => new Step
        {
            Name = name,
            Executor = executor
        };
    }
}
