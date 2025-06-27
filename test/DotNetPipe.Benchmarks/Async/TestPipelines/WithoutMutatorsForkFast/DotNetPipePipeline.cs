using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Async;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsForkFast;

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
            .BuildPipeline().Compile();
        return pipeline;
    }

    public async Task Run(string input)
    {
        await _handler(input);
    }
}
