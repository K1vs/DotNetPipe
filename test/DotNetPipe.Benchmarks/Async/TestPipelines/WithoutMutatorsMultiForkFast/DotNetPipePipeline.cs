using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsMultiForkFast;

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
            .BuildPipeline().Compile();

        return pipeline;
    }

    public async Task Run(string input)
    {
        await _handler(input);
    }
}
