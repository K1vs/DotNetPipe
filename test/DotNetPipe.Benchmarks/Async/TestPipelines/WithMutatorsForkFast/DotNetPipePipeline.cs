using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Async;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsForkFast;

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
        var pipeline = Pipelines.CreateAsyncPipeline<string>("TestForkPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenFork<string, string>("DigitContentFork", async (input, digitBranch, nonDigitBranch) =>
            {
                // Check if string contains only digits (after trimming)
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    await digitBranch(input);
                }
                else
                {
                    await nonDigitBranch(input);
                }
            },
            // Digit processing branch
            space => space.CreatePipeline<string>("DigitProcessing")
                .StartWithLinear<string>("RemoveNonDigits", async (input, next) =>
                {
                    var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                    await next(digitsOnly);
                })
                .ThenLinear<int>("ParseToInt", async (input, next) =>
                {
                    if (int.TryParse(input, out var number))
                    {
                        await next(number);
                    }
                    else
                    {
                        await next(0); // Default to 0 if parsing fails
                    }
                })
                .HandleWith("IntHandler", async (input) => _consumer.Consume(input))
                .BuildPipeline(),
            // Non-digit processing branch
            space => space.CreatePipeline<string>("NonDigitProcessing")
                .StartWithLinear<string>("RemoveDigits", async (input, next) =>
                {
                    var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                    await next(nonDigitsOnly);
                })
                .ThenLinear<string>("AddSpaces", async (input, next) =>
                {
                    var withSpaces = $"  {input}  ";
                    await next(withSpaces);
                })
                .HandleWith("StringHandler", async (input) => _consumer.Consume(input))
                .BuildPipeline())
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureForkPipelineMutators);
            });
        return pipeline;
    }

    private void ConfigureForkPipelineMutators(Space space)
    {
        // Mutator for TrimString step - pass through as-is
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for RemoveNonDigits step in DigitProcessing - pass through as-is
        var removeNonDigitsStep = space.GetRequiredLinearStep<string, string, string>("DigitProcessing", "RemoveNonDigits");
        var removeNonDigitsMutator = new StepMutator<Pipe<string, string>>("RemoveNonDigitsMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        removeNonDigitsStep.Mutators.AddMutator(removeNonDigitsMutator, AddingMode.ExactPlace);

        // Mutator for ParseToInt step in DigitProcessing - add 5 to the parsed value
        var parseToIntStep = space.GetRequiredLinearStep<string, string, int>("DigitProcessing", "ParseToInt");
        var parseToIntMutator = new StepMutator<Pipe<string, int>>("ParseToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    // Mutation: add 5 to the parsed value
                    await next(number + 5);
                }
                else
                {
                    await next(0); // Default to 0 if parsing fails
                }
            };
        });
        parseToIntStep.Mutators.AddMutator(parseToIntMutator, AddingMode.ExactPlace);

        // Mutator for RemoveDigits step in NonDigitProcessing - pass through as-is
        var removeDigitsStep = space.GetRequiredLinearStep<string, string, string>("NonDigitProcessing", "RemoveDigits");
        var removeDigitsMutator = new StepMutator<Pipe<string, string>>("RemoveDigitsMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        removeDigitsStep.Mutators.AddMutator(removeDigitsMutator, AddingMode.ExactPlace);

        // Mutator for AddSpaces step in NonDigitProcessing - use asterisks instead of spaces
        var addSpacesStep = space.GetRequiredLinearStep<string, string, string>("NonDigitProcessing", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, string>>("AddSpacesMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Mutation: use asterisks instead of spaces
                var withAsterisks = $"**{input}**";
                await next(withAsterisks);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        // Mutators for handlers - pass through as-is
        var intHandlerStep = space.GetRequiredHandlerStep<string, int>("DigitProcessing", "IntHandler");
        var intHandlerMutator = new StepMutator<Handler<int>>("IntHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        intHandlerStep.Mutators.AddMutator(intHandlerMutator, AddingMode.ExactPlace);

        var stringHandlerStep = space.GetRequiredHandlerStep<string, string>("NonDigitProcessing", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string>>("StringHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);
    }

    public async Task Run(string input)
    {
        await _handler(input);
    }
}
