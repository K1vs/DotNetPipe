using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Async;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsIfElseFast;

internal class DotNetPipePipeline
{
    private const int _multiplier = 2;
    private const int _constantToAdd = 4;
    private readonly Handler<string> _handler;
    private readonly Consumer _consumer = new();

    public DotNetPipePipeline()
    {
        _handler = CreateHandler();
    }

    private Handler<string> CreateHandler()
    {
        var pipeline = Pipelines.CreateAsyncPipeline<string>("TestIfElseStepPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenIfElse<string, int, int>("CheckIntOrFloat", async (input, trueNext, falseNext) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, we actually want to bypass both branches and continue directly
                    // But since IfElse requires going through one of the branches, we'll use false branch for int
                    await falseNext(intValue);
                }
                else
                {
                    // If not an int, go to true branch (for float parsing)
                    await trueNext(input);
                }
            },
            // True branch - float processing
            space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", async (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        await next(floatValue);
                    }
                })
                .ThenLinear<int>("RoundToInt", async (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    await next(rounded);
                })
                .BuildOpenPipeline(),
            // False branch - multiply by multiplier
            space => space.CreatePipeline<int>("IntOrDefaultProcessing")
                .StartWithLinear<int>("ParseIntOrDefault", async (input, next) =>
                {
                    // If we got here, it means we parsed as int in the false branch
                    // So we just pass it through
                    await next(input * _multiplier);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + _constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();
        return pipeline;
    }

    public async Task Run(string input)
    {
        await _handler(input);
    }
}
