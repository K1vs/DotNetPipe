using BenchmarkDotNet.Engines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsForkFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public async Task Run(string input)
    {
        await RunInternal(input);
    }

    protected virtual async Task RunInternal(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: DigitContentFork
        await DigitContentFork(trimmed);
    }

    protected virtual async Task<string> TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual async Task DigitContentFork(string input)
    {
        // Check if string contains only digits (after trimming)
        var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

        if (containsOnlyDigits)
        {
            await DigitProcessing(input);
        }
        else
        {
            await NonDigitProcessing(input);
        }
    }

    protected virtual async Task DigitProcessing(string input)
    {
        // RemoveNonDigits step (though input should already be digits only)
        var digitsOnly = await RemoveNonDigits(input);

        // ParseToInt step
        var intResult = await ParseToInt(digitsOnly);

        // IntHandler
        await IntHandler(intResult);
    }

    protected virtual async Task NonDigitProcessing(string input)
    {
        // RemoveDigits step
        var nonDigitsOnly = await RemoveDigits(input);

        // AddSpaces step
        var withSpaces = await AddSpaces(nonDigitsOnly);

        // StringHandler
        await StringHandler(withSpaces);
    }

    protected virtual async Task<string> RemoveNonDigits(string input)
    {
        return new string(input.Where(char.IsDigit).ToArray());
    }

    protected virtual async Task<int> ParseToInt(string input)
    {
        if (int.TryParse(input, out var number))
        {
            return number;
        }
        else
        {
            return 0; // Default to 0 if parsing fails
        }
    }

    protected virtual async Task<string> RemoveDigits(string input)
    {
        return new string(input.Where(c => !char.IsDigit(c)).ToArray());
    }

    protected virtual async Task<string> AddSpaces(string input)
    {
        return $"  {input}  ";
    }

    protected virtual async Task IntHandler(int input)
    {
        _consumer.Consume(input);
    }

    protected virtual async Task StringHandler(string input)
    {
        _consumer.Consume(input);
    }
}
