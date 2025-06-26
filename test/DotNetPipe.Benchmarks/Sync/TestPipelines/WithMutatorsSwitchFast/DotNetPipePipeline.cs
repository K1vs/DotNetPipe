using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Sync;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsSwitchFast;

internal class DotNetPipePipeline
{
    private readonly Handler<string> _handler;
    private readonly Consumer _consumer = new();

    public DotNetPipePipeline()
    {
        _handler = CreateHandler();
    }

    private Handler<string> CreateHandler()
    {
        var space = Pipelines.CreateSyncSpace();
        var defaultPipeline = space.CreatePipeline<int>("StringLengthPipeline")
            .StartWithLinear<int>("IdentityOperation", (input, next) =>
            {
                next(input); // Use string length as-is
            })
            .BuildOpenPipeline();

        var pipeline = space.CreatePipeline<string>("TestSwitchPipeline")
            .StartWithLinear<string>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                next(trimmed);
            })
            .ThenSwitch<int, int, int>("NumberRangeSwitch", (input, cases, defaultNext) =>
            {
                // Try to parse as integer
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        cases["BetweenZeroAndHundred"](number);
                    }
                    else if (number < 0)
                    {
                        cases["LessThanZero"](number);
                    }
                    else // number == 0
                    {
                        cases["EqualToZero"](number);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    defaultNext(stringLength);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int>("MultiplyByThree")
                    .StartWithLinear<int>("MultiplyOperation", (input, next) =>
                    {
                        var result = input * 3;
                        next(result);
                    })
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int>("AddTwo")
                    .StartWithLinear<int>("AddOperation", (input, next) =>
                    {
                        var result = input + 2;
                        next(result);
                    })
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int>("MultiplyByTwo")
                    .StartWithLinear<int>("MultiplyOperation", (input, next) =>
                    {
                        var result = input * 2;
                        next(result);
                    })
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int>("KeepZero")
                    .StartWithLinear<int>("IdentityOperation", (input, next) =>
                    {
                        next(input); // Keep the same value (0)
                    })
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", (input) => _consumer.Consume(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureSwitchPipelineMutators);
            });
        return pipeline;
    }

    private void ConfigureSwitchPipelineMutators(Space space)
    {
        // Mutator for TrimString step - pass through as-is
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestSwitchPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for the Switch step selector - modify the logic to change thresholds
        var switchStep = space.GetRequiredSwitchStep<string, string, int, int, int>("TestSwitchPipeline", "NumberRangeSwitch");
        var switchSelectorMutator = new StepMutator<SwitchSelector<string, int, int>>("NumberRangeSwitchMutator", 1, (selector) =>
        {
            return (input, cases, defaultNext) =>
            {
                // Mutated logic: change the thresholds (50 instead of 100, <= 0 instead of < 0)
                if (int.TryParse(input, out var number))
                {
                    if (number > 50) // was 100
                    {
                        cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        cases["BetweenZeroAndHundred"](number);
                    }
                    else if (number < 0)
                    {
                        cases["LessThanZero"](number);
                    }
                    else // number == 0
                    {
                        cases["EqualToZero"](number);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    defaultNext(stringLength);
                }
            };
        });
        switchStep.Mutators.AddMutator(switchSelectorMutator, AddingMode.ExactPlace);

        // Mutator for GreaterThan100 branch - add 3 before multiplying
        var greaterThan100Step = space.GetRequiredLinearStep<int, int, int>("MultiplyByThree", "MultiplyOperation");
        var greaterThan100Mutator = new StepMutator<Pipe<int, int>>("MultiplyByThreeMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                // Mutation: add 3 before multiplying by 3
                input += 3;
                var result = input * 3;
                next(result);
            };
        });
        greaterThan100Step.Mutators.AddMutator(greaterThan100Mutator, AddingMode.ExactPlace);

        // Mutator for BetweenZeroAndHundred branch - pass through as-is
        var betweenStep = space.GetRequiredLinearStep<int, int, int>("AddTwo", "AddOperation");
        var betweenMutator = new StepMutator<Pipe<int, int>>("AddTwoMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                pipe(input, next);
            };
        });
        betweenStep.Mutators.AddMutator(betweenMutator, AddingMode.ExactPlace);

        // Mutator for LessThanZero branch - pass through as-is
        var lessThanZeroStep = space.GetRequiredLinearStep<int, int, int>("MultiplyByTwo", "MultiplyOperation");
        var lessThanZeroMutator = new StepMutator<Pipe<int, int>>("MultiplyByTwoMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                pipe(input, next);
            };
        });
        lessThanZeroStep.Mutators.AddMutator(lessThanZeroMutator, AddingMode.ExactPlace);

        // Mutator for EqualToZero branch - pass through as-is
        var equalToZeroStep = space.GetRequiredLinearStep<int, int, int>("KeepZero", "IdentityOperation");
        var equalToZeroMutator = new StepMutator<Pipe<int, int>>("KeepZeroMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                pipe(input, next);
            };
        });
        equalToZeroStep.Mutators.AddMutator(equalToZeroMutator, AddingMode.ExactPlace);

        // Mutator for StringLengthPipeline default branch - add 3
        var defaultStep = space.GetRequiredLinearStep<int, int, int>("StringLengthPipeline", "IdentityOperation");
        var defaultMutator = new StepMutator<Pipe<int, int>>("IdentityOperationMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                // Mutation: add 3 to string length
                input += 3;
                next(input);
            };
        });
        defaultStep.Mutators.AddMutator(defaultMutator, AddingMode.ExactPlace);

        // Mutator for handler step - pass through as-is
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestSwitchPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    public void Run(string input)
    {
        _handler(input);
    }
}
