using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Universal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsSwitchFast;

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
        var space = new Space();
        var defaultPipeline = space.CreatePipeline<int>("StringLengthPipeline")
            .StartWithLinear<int>("IdentityOperation", async (input, next) =>
            {
                await next(input); // Use string length as-is
            })
            .BuildOpenPipeline();

        var pipeline = Pipelines.CreatePipeline<string>("TestSwitchPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenSwitch<int, int, int>("NumberRangeSwitch", async (input, cases, defaultNext) =>
            {
                // Try to parse as integer
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        await cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        await cases["BetweenZeroAndHundred"](number);
                    }
                    else if (number < 0)
                    {
                        await cases["LessThanZero"](number);
                    }
                    else // number == 0
                    {
                        await cases["EqualToZero"](number);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    await defaultNext(stringLength);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int>("MultiplyByThree")
                    .StartWithLinear<int>("MultiplyOperation", async (input, next) =>
                    {
                        var result = input * 3;
                        await next(result);
                    })
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int>("AddTwo")
                    .StartWithLinear<int>("AddOperation", async (input, next) =>
                    {
                        var result = input + 2;
                        await next(result);
                    })
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int>("MultiplyByTwo")
                    .StartWithLinear<int>("MultiplyOperation", async (input, next) =>
                    {
                        var result = input * 2;
                        await next(result);
                    })
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int>("KeepZero")
                    .StartWithLinear<int>("IdentityOperation", async (input, next) =>
                    {
                        await next(input); // Keep the same value (0)
                    })
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", async (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();

        return pipeline;
    }

    public async ValueTask Run(string input)
    {
        await _handler(input);
    }
}
