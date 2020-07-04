# nLoad
Simple and easy to use framework for performing load tests in .net core

```c#
    class Program
    {
        static void Main(string[] args)
        {
            var scenario = Scenario.CreateScenario(
                "test",
                new System.Collections.Generic.List<Step>
                {
                    Step.Create("Step1", Step1),
                    Step.Create("Step2", Step2),
                    Step.Create("Step3", Step3),
                },
                null,
                10,
                10,
                null);
            LoadRunner.RegisterScenarios(scenario).RunInConsole().Wait();
        }

        public static Response Step1(StepContext context)
        {
            Thread.Sleep(1000);
            return Response.Ok(context.Data);
        }

        public static Response Step2(StepContext context)
        {
            Thread.Sleep(2000);
            return Response.Ok(context.Data);
        }

        public static Response Step3(StepContext context)
        {
            Thread.Sleep(5000);
            return Response.Ok(context.Data);
        }
    }
```
![Load test results](https://github.com/bytebarian/nLoad/blob/master/loadtest.PNG)
