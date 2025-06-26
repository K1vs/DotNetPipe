using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsMultiForkFast;

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

        // Create sub-pipelines before main pipeline
        space.CreatePipeline<string>("DigitProcessingPipeline")
            .StartWithLinear<int>("ParseStringToInt", (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    next(number);
                }
                else
                {
                    next(0); // Default value if parsing fails
                }
            })
            .ThenLinear<int>("AddConstant", (input, next) =>
            {
                var result = input + 10; // Add constant 10
                next(result);
            })
            .HandleWith("IntHandler", (input) => _consumer.Consume(input))
            .BuildPipeline();

        space.CreatePipeline<string>("LetterProcessingPipeline")
            .StartWithLinear<string>("AddSpaces", (input, next) =>
            {
                var withSpaces = $"  {input}  ";
                next(withSpaces);
            })
            .HandleWith("StringHandler", (input) => _consumer.Consume(input))
            .BuildPipeline();

        space.CreatePipeline<string>("SpecialCharProcessingPipeline")
            .StartWithLinear<string>("RemoveWhitespace", (input, next) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                next(noWhitespace);
            })
            .ThenLinear<char[]>("ConvertToCharArray", (input, next) =>
            {
                var charArray = input.ToCharArray();
                next(charArray);
            })
            .ThenLinear<char[]>("RemoveDuplicates", (input, next) =>
            {
                var uniqueChars = input.Distinct().ToArray();
                next(uniqueChars);
            })
            .HandleWith("CharArrayHandler", (input) => _consumer.Consume(input))
            .BuildPipeline();

        space.CreatePipeline<char[]>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount)>("CountDigitsAndLetters", (input, next) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                next((digitCount, letterCount));
            })
            .ThenLinear<int>("CalculateRatio", (input, next) =>
            {
                // Calculate ratio of digits to letters (floor division)
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                next(ratio);
            })
            .HandleWith("IntHandler", async (input) => _consumer.Consume(input))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string>("TestMultiForkPipeline")
            .StartWithLinear<string>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                next(trimmed);
            })
            .ThenMultiFork<string, char[]>("ClassifyStringContent", (input, branches, defaultNext) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (containsOnlyDigits)
                {
                    branches["DigitBranch"](input);
                }
                else if (containsOnlyLetters)
                {
                    branches["LetterBranch"](input);
                }
                else if (containsOnlySpecialChars)
                {
                    branches["SpecialCharBranch"](input);
                }
                else
                {
                    // Mixed content -> default pipeline
                    var charArray = input.ToCharArray();
                    defaultNext(charArray);
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
            return (input, next) =>
            {
                pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for ClassifyStringContent selector - modify the classification logic
        var multiForkStep = space.GetRequiredMultiForkStep<string, string, string, char[]>("TestMultiForkPipeline", "ClassifyStringContent");
        var multiForkSelectorMutator = new StepMutator<MultiForkSelector<string, string, char[]>>("ClassifyStringContentMutator", 1, (selector) =>
        {
            return (input, branches, defaultNext) =>
            {
                // Modified logic: always treat everything as special characters unless it's digits only
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    // Only digits go to digit branch
                    branches["DigitBranch"](input);
                }
                else
                {
                    // Everything else (letters, special chars, mixed) goes to special char branch
                    branches["SpecialCharBranch"](input);
                }
            };
        });
        multiForkStep.Mutators.AddMutator(multiForkSelectorMutator, AddingMode.ExactPlace);

        // Mutator for ParseStringToInt step in DigitProcessingPipeline - pass through as-is
        var parseStringToIntStep = space.GetRequiredLinearStep<string, string, int>("DigitProcessingPipeline", "ParseStringToInt");
        var parseStringToIntMutator = new StepMutator<Pipe<string, int>>("ParseStringToIntMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                pipe(input, next);
            };
        });
        parseStringToIntStep.Mutators.AddMutator(parseStringToIntMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step in DigitProcessingPipeline - add 5 more to the result
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("DigitProcessingPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                var result = input + 10 + 5; // Original +10, mutation adds +5 more
                next(result);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for AddSpaces step in LetterProcessingPipeline - use asterisks instead of spaces
        var addSpacesStep = space.GetRequiredLinearStep<string, string, string>("LetterProcessingPipeline", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, string>>("AddSpacesMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
                next(withAsterisks);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        // Mutator for RemoveWhitespace step in SpecialCharProcessingPipeline - pass through as-is
        var removeWhitespaceStep = space.GetRequiredLinearStep<string, string, string>("SpecialCharProcessingPipeline", "RemoveWhitespace");
        var removeWhitespaceMutator = new StepMutator<Pipe<string, string>>("RemoveWhitespaceMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                pipe(input, next);
            };
        });
        removeWhitespaceStep.Mutators.AddMutator(removeWhitespaceMutator, AddingMode.ExactPlace);

        // Mutator for ConvertToCharArray step in SpecialCharProcessingPipeline - add underscore
        var convertToCharArrayStep = space.GetRequiredLinearStep<string, string, char[]>("SpecialCharProcessingPipeline", "ConvertToCharArray");
        var convertToCharArrayMutator = new StepMutator<Pipe<string, char[]>>("ConvertToCharArrayMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                var inputWithUnderscore = input + "_"; // Add underscore before converting
                var charArray = inputWithUnderscore.ToCharArray();
                next(charArray);
            };
        });
        convertToCharArrayStep.Mutators.AddMutator(convertToCharArrayMutator, AddingMode.ExactPlace);

        // Mutator for RemoveDuplicates step in SpecialCharProcessingPipeline - pass through as-is
        var removeDuplicatesStep = space.GetRequiredLinearStep<string, char[], char[]>("SpecialCharProcessingPipeline", "RemoveDuplicates");
        var removeDuplicatesMutator = new StepMutator<Pipe<char[], char[]>>("RemoveDuplicatesMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                pipe(input, next);
            };
        });
        removeDuplicatesStep.Mutators.AddMutator(removeDuplicatesMutator, AddingMode.ExactPlace);

        // Mutator for CountDigitsAndLetters step in DefaultProcessingPipeline - pass through as-is
        var countStep = space.GetRequiredLinearStep<char[], char[], (int DigitCount, int LetterCount)>("DefaultProcessingPipeline", "CountDigitsAndLetters");
        var countMutator = new StepMutator<Pipe<char[], (int DigitCount, int LetterCount)>>("CountDigitsAndLettersMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                pipe(input, next);
            };
        });
        countStep.Mutators.AddMutator(countMutator, AddingMode.ExactPlace);

        // Mutator for CalculateRatio step in DefaultProcessingPipeline - add 2 to the ratio
        var calculateRatioStep = space.GetRequiredLinearStep<char[], (int DigitCount, int LetterCount), int>("DefaultProcessingPipeline", "CalculateRatio");
        var calculateRatioMutator = new StepMutator<Pipe<(int DigitCount, int LetterCount), int>>("CalculateRatioMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                // Calculate ratio of digits to letters (floor division) and add 2
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                next(ratio + 2); // Add 2 to the calculated ratio
            };
        });
        calculateRatioStep.Mutators.AddMutator(calculateRatioMutator, AddingMode.ExactPlace);

        // Mutators for handlers - pass through as-is
        var intHandlerStepDigit = space.GetRequiredHandlerStep<string, int>("DigitProcessingPipeline", "IntHandler");
        var intHandlerMutatorDigit = new StepMutator<Handler<int>>("IntHandlerMutatorDigit", 1, (handler) =>
        {
            return (input) =>
            {
                handler(input);
            };
        });
        intHandlerStepDigit.Mutators.AddMutator(intHandlerMutatorDigit, AddingMode.ExactPlace);

        var stringHandlerStep = space.GetRequiredHandlerStep<string, string>("LetterProcessingPipeline", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string>>("StringHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                handler(input);
            };
        });
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);

        var charArrayHandlerStep = space.GetRequiredHandlerStep<string, char[]>("SpecialCharProcessingPipeline", "CharArrayHandler");
        var charArrayHandlerMutator = new StepMutator<Handler<char[]>>("CharArrayHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                handler(input);
            };
        });
        charArrayHandlerStep.Mutators.AddMutator(charArrayHandlerMutator, AddingMode.ExactPlace);

        var intHandlerStepDefault = space.GetRequiredHandlerStep<char[], int>("DefaultProcessingPipeline", "IntHandler");
        var intHandlerMutatorDefault = new StepMutator<Handler<int>>("IntHandlerMutatorDefault", 1, (handler) =>
        {
            return (input) =>
            {
                handler(input);
            };
        });
        intHandlerStepDefault.Mutators.AddMutator(intHandlerMutatorDefault, AddingMode.ExactPlace);
    }

    public void Run(string input)
    {
        _handler(input);
    }
}
