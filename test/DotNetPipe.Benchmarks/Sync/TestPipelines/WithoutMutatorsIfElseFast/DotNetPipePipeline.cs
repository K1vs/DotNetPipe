using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Sync;
using System;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsIfElseFast;

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
        var pipeline = Pipelines.CreateSyncPipeline<string>("TestIfElseStepPipeline")
            .StartWithLinear<string>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                next(trimmed);
            })
            .ThenIfElse<string, int, int>("CheckIntOrFloat", (input, trueNext, falseNext) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, we actually want to bypass both branches and continue directly
                    // But since IfElse requires going through one of the branches, we'll use false branch for int
                    falseNext(intValue);
                }
                else
                {
                    // If not an int, go to true branch (for float parsing)
                    trueNext(input);
                }
            },
            // True branch - float processing
            space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        next(floatValue);
                    }
                })
                .ThenLinear<int>("RoundToInt", (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    next(rounded);
                })
                .BuildOpenPipeline(),
            // False branch - multiply by multiplier
            space => space.CreatePipeline<int>("IntOrDefaultProcessing")
                .StartWithLinear<int>("ParseIntOrDefault", (input, next) =>
                {
                    // If we got here, it means we parsed as int in the false branch
                    // So we just pass it through
                    next(input * _multiplier);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", (input, next) =>
            {
                var result = input + _constantToAdd;
                next(result);
            })
            .HandleWith("TestHandler", (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();
        return pipeline;
    }

    public void Run(string input)
    {
        _handler(input);
    }
}
