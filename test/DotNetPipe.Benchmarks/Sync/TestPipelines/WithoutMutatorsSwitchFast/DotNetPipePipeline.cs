using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Sync;
using System;
using System.Collections.Generic;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsSwitchFast;

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

        var pipeline = Pipelines.CreateSyncPipeline<string>("TestSwitchPipeline")
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
            .BuildPipeline().Compile();

        return pipeline;
    }

    public void Run(string input)
    {
        _handler(input);
    }
}
