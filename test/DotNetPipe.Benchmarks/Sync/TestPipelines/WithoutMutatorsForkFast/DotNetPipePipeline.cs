using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Sync;
using System;
using System.Linq;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsForkFast;

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
        var pipeline = Pipelines.CreateSyncPipeline<string>("TestForkPipeline")
            .StartWithLinear<string>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                next(trimmed);
            })
            .ThenFork<string, string>("DigitContentFork", (input, digitBranch, nonDigitBranch) =>
            {
                // Check if string contains only digits (after trimming)
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    digitBranch(input);
                }
                else
                {
                    nonDigitBranch(input);
                }
            },
            // Digit processing branch
            space => space.CreatePipeline<string>("DigitProcessing")
                .StartWithLinear<string>("RemoveNonDigits", (input, next) =>
                {
                    var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                    next(digitsOnly);
                })
                .ThenLinear<int>("ParseToInt", (input, next) =>
                {
                    if (int.TryParse(input, out var number))
                    {
                        next(number);
                    }
                    else
                    {
                        next(0); // Default to 0 if parsing fails
                    }
                })
                .HandleWith("IntHandler", (input) => _consumer.Consume(input))
                .BuildPipeline(),
            // Non-digit processing branch
            space => space.CreatePipeline<string>("NonDigitProcessing")
                .StartWithLinear<string>("RemoveDigits", (input, next) =>
                {
                    var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                    next(nonDigitsOnly);
                })
                .ThenLinear<string>("AddSpaces", (input, next) =>
                {
                    var withSpaces = $"  {input}  ";
                    next(withSpaces);
                })
                .HandleWith("StringHandler", (input) => _consumer.Consume(input))
                .BuildPipeline())
            .BuildPipeline().Compile();
        return pipeline;
    }

    public void Run(string input)
    {
        _handler(input);
    }
}
