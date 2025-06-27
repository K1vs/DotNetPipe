using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsMultiForkFast;

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
        var space = Pipelines.CreateAsyncSpace();

        // Create sub-pipelines before main pipeline
        space.CreatePipeline<string>("DigitProcessingPipeline")
            .StartWithLinear<int>("ParseStringToInt", async (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    await next(number);
                }
                else
                {
                    await next(0); // Default value if parsing fails
                }
            })
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + 10; // Add constant 10
                await next(result);
            })
            .HandleWith("IntHandler", async (input) => _consumer.Consume(input))
            .BuildPipeline();

        space.CreatePipeline<string>("LetterProcessingPipeline")
            .StartWithLinear<string>("AddSpaces", async (input, next) =>
            {
                var withSpaces = $"  {input}  ";
                await next(withSpaces);
            })
            .HandleWith("StringHandler", async (input) => _consumer.Consume(input))
            .BuildPipeline();

        space.CreatePipeline<string>("SpecialCharProcessingPipeline")
            .StartWithLinear<string>("RemoveWhitespace", async (input, next) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                await next(noWhitespace);
            })
            .ThenLinear<char[]>("ConvertToCharArray", async (input, next) =>
            {
                var charArray = input.ToCharArray();
                await next(charArray);
            })
            .ThenLinear<char[]>("RemoveDuplicates", async (input, next) =>
            {
                var uniqueChars = input.Distinct().ToArray();
                await next(uniqueChars);
            })
            .HandleWith("CharArrayHandler", async (input) => _consumer.Consume(input))
            .BuildPipeline();

        space.CreatePipeline<char[]>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount)>("CountDigitsAndLetters", async (input, next) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                await next((digitCount, letterCount));
            })
            .ThenLinear<int>("CalculateRatio", async (input, next) =>
            {
                // Calculate ratio of digits to letters (floor division)
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                await next(ratio);
            })
            .HandleWith("IntHandler", async (input) => _consumer.Consume(input))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string>("TestMultiForkPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenMultiFork<string, char[]>("ClassifyStringContent", async (input, branches, defaultNext) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (containsOnlyDigits)
                {
                    await branches["DigitBranch"](input);
                }
                else if (containsOnlyLetters)
                {
                    await branches["LetterBranch"](input);
                }
                else if (containsOnlySpecialChars)
                {
                    await branches["SpecialCharBranch"](input);
                }
                else
                {
                    // Mixed content -> default pipeline
                    var charArray = input.ToCharArray();
                    await defaultNext(charArray);
                }
            },
            space => new Dictionary<string, Pipeline<string>>
            {
                ["DigitBranch"] = space.GetPipeline<string>("DigitProcessingPipeline")!,
                ["LetterBranch"] = space.GetPipeline<string>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = space.GetPipeline<string>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            space => space.GetPipeline<char[]>("DefaultProcessingPipeline")!)
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigurePipelineMutators);
            });

        return pipeline;
    }

    private void ConfigurePipelineMutators(Space space)
    {
        // Mutator for TrimString step - pass through as-is
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestMultiForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for ClassifyStringContent selector - modify the classification logic
        var multiForkStep = space.GetRequiredMultiForkStep<string, string, string, char[]>("TestMultiForkPipeline", "ClassifyStringContent");
        var multiForkSelectorMutator = new StepMutator<MultiForkSelector<string, string, char[]>>("ClassifyStringContentMutator", 1, (selector) =>
        {
            return async (input, branches, defaultNext) =>
            {
                // Modified logic: always treat everything as special characters unless it's digits only
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    // Only digits go to digit branch
                    await branches["DigitBranch"](input);
                }
                else
                {
                    // Everything else (letters, special chars, mixed) goes to special char branch
                    await branches["SpecialCharBranch"](input);
                }
            };
        });
        multiForkStep.Mutators.AddMutator(multiForkSelectorMutator, AddingMode.ExactPlace);

        // Mutator for ParseStringToInt step in DigitProcessingPipeline - pass through as-is
        var parseStringToIntStep = space.GetRequiredLinearStep<string, string, int>("DigitProcessingPipeline", "ParseStringToInt");
        var parseStringToIntMutator = new StepMutator<Pipe<string, int>>("ParseStringToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        parseStringToIntStep.Mutators.AddMutator(parseStringToIntMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step in DigitProcessingPipeline - add 5 more to the result
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("DigitProcessingPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var result = input + 10 + 5; // Original +10, mutation adds +5 more
                await next(result);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for AddSpaces step in LetterProcessingPipeline - use asterisks instead of spaces
        var addSpacesStep = space.GetRequiredLinearStep<string, string, string>("LetterProcessingPipeline", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, string>>("AddSpacesMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
                await next(withAsterisks);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        // Mutator for RemoveWhitespace step in SpecialCharProcessingPipeline - pass through as-is
        var removeWhitespaceStep = space.GetRequiredLinearStep<string, string, string>("SpecialCharProcessingPipeline", "RemoveWhitespace");
        var removeWhitespaceMutator = new StepMutator<Pipe<string, string>>("RemoveWhitespaceMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        removeWhitespaceStep.Mutators.AddMutator(removeWhitespaceMutator, AddingMode.ExactPlace);

        // Mutator for ConvertToCharArray step in SpecialCharProcessingPipeline - add underscore
        var convertToCharArrayStep = space.GetRequiredLinearStep<string, string, char[]>("SpecialCharProcessingPipeline", "ConvertToCharArray");
        var convertToCharArrayMutator = new StepMutator<Pipe<string, char[]>>("ConvertToCharArrayMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var inputWithUnderscore = input + "_"; // Add underscore before converting
                var charArray = inputWithUnderscore.ToCharArray();
                await next(charArray);
            };
        });
        convertToCharArrayStep.Mutators.AddMutator(convertToCharArrayMutator, AddingMode.ExactPlace);

        // Mutator for RemoveDuplicates step in SpecialCharProcessingPipeline - pass through as-is
        var removeDuplicatesStep = space.GetRequiredLinearStep<string, char[], char[]>("SpecialCharProcessingPipeline", "RemoveDuplicates");
        var removeDuplicatesMutator = new StepMutator<Pipe<char[], char[]>>("RemoveDuplicatesMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        removeDuplicatesStep.Mutators.AddMutator(removeDuplicatesMutator, AddingMode.ExactPlace);

        // Mutator for CountDigitsAndLetters step in DefaultProcessingPipeline - pass through as-is
        var countStep = space.GetRequiredLinearStep<char[], char[], (int DigitCount, int LetterCount)>("DefaultProcessingPipeline", "CountDigitsAndLetters");
        var countMutator = new StepMutator<Pipe<char[], (int DigitCount, int LetterCount)>>("CountDigitsAndLettersMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        countStep.Mutators.AddMutator(countMutator, AddingMode.ExactPlace);

        // Mutator for CalculateRatio step in DefaultProcessingPipeline - add 2 to the ratio
        var calculateRatioStep = space.GetRequiredLinearStep<char[], (int DigitCount, int LetterCount), int>("DefaultProcessingPipeline", "CalculateRatio");
        var calculateRatioMutator = new StepMutator<Pipe<(int DigitCount, int LetterCount), int>>("CalculateRatioMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Calculate ratio of digits to letters (floor division) and add 2
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                await next(ratio + 2); // Add 2 to the calculated ratio
            };
        });
        calculateRatioStep.Mutators.AddMutator(calculateRatioMutator, AddingMode.ExactPlace);

        // Mutators for handlers - pass through as-is
        var intHandlerStepDigit = space.GetRequiredHandlerStep<string, int>("DigitProcessingPipeline", "IntHandler");
        var intHandlerMutatorDigit = new StepMutator<Handler<int>>("IntHandlerMutatorDigit", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        intHandlerStepDigit.Mutators.AddMutator(intHandlerMutatorDigit, AddingMode.ExactPlace);

        var stringHandlerStep = space.GetRequiredHandlerStep<string, string>("LetterProcessingPipeline", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string>>("StringHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);

        var charArrayHandlerStep = space.GetRequiredHandlerStep<string, char[]>("SpecialCharProcessingPipeline", "CharArrayHandler");
        var charArrayHandlerMutator = new StepMutator<Handler<char[]>>("CharArrayHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        charArrayHandlerStep.Mutators.AddMutator(charArrayHandlerMutator, AddingMode.ExactPlace);

        var intHandlerStepDefault = space.GetRequiredHandlerStep<char[], int>("DefaultProcessingPipeline", "IntHandler");
        var intHandlerMutatorDefault = new StepMutator<Handler<int>>("IntHandlerMutatorDefault", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        intHandlerStepDefault.Mutators.AddMutator(intHandlerMutatorDefault, AddingMode.ExactPlace);
    }

    public async Task Run(string input)
    {
        await _handler(input);
    }
}
