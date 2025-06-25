using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Universal;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithMutatorsIfElseFast;

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
        var pipeline = Pipelines.CreatePipeline<string>("TestIfElseStepPipeline")
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureIfElseStepPipelineMutators);
            });
        return pipeline;
    }

    private void ConfigureIfElseStepPipelineMutators(Space space)
    {
        // Mutator for TrimString step - pass through as-is
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestIfElseStepPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for the IfElse step selector - modify the logic to swap the branches
        var ifElseStep = space.GetRequiredIfElseStep<string, string, string, int, int>("TestIfElseStepPipeline", "CheckIntOrFloat");
        var ifElseSelectorMutator = new StepMutator<IfElseSelector<string, string, int>>("CheckIntOrFloatMutator", 1, (selector) =>
        {
            return async (input, trueNext, falseNext) =>
            {
                // Modified logic: swap the branches - what was true becomes false and vice versa
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, go to true branch (was false branch before)
                    // Now int values go to float processing pipeline
                    await trueNext(intValue.ToString());
                }
                else
                {
                    // If not an int, go to false branch (was true branch before)
                    // This creates a mutation that changes the routing logic
                    await falseNext(0); // Default value since we can't parse
                }
            };
        });
        ifElseStep.Mutators.AddMutator(ifElseSelectorMutator, AddingMode.ExactPlace);

        // Mutator for ParseFloat step in FloatProcessing - pass through as-is
        var parseFloatStep = space.GetRequiredLinearStep<string, string, double>("FloatProcessing", "ParseFloat");
        var parseFloatMutator = new StepMutator<Pipe<string, double>>("ParseFloatMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        parseFloatStep.Mutators.AddMutator(parseFloatMutator, AddingMode.ExactPlace);

        // Mutator for the RoundToInt step in the FloatProcessing pipeline - add 1 to input before rounding
        var roundStep = space.GetRequiredLinearStep<string, double, int>("FloatProcessing", "RoundToInt");
        var roundMutator = new StepMutator<Pipe<double, int>>("RoundToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Add 1 to the input before rounding
                input += 1;
                var rounded = (int)Math.Round(input);
                await next(rounded);
            };
        });
        roundStep.Mutators.AddMutator(roundMutator, AddingMode.ExactPlace);

        // Mutator for ParseIntOrDefault step in the IntOrDefaultProcessing pipeline - multiply by multiplier + 2
        var parseIntStep = space.GetRequiredLinearStep<int, int, int>("IntOrDefaultProcessing", "ParseIntOrDefault");
        var parseIntMutator = new StepMutator<Pipe<int, int>>("ParseIntOrDefaultMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply mutation: multiply by (multiplier + 2) instead of just multiplier
                await next(input * (_multiplier + 2));
            };
        });
        parseIntStep.Mutators.AddMutator(parseIntMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step - pass through as-is
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("TestIfElseStepPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1 to the final result
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestIfElseStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    public async ValueTask Run(string input)
    {
        await _handler(input);
    }
}
