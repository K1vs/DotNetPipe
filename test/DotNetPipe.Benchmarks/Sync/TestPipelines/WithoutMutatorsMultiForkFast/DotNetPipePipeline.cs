using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Sync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsMultiForkFast;

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
            .HandleWith("IntHandler", (input) => _consumer.Consume(input))
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
            .BuildPipeline().Compile();

        return pipeline;
    }

    public void Run(string input)
    {
        _handler(input);
    }
}
