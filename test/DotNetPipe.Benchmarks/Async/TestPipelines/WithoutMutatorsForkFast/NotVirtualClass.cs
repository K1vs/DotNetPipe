using BenchmarkDotNet.Engines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsForkFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public async Task Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: DigitContentFork
        await DigitContentFork(trimmed);
    }

    private async Task<string> TrimString(string input)
    {
        return input.Trim();
    }

    private async Task DigitContentFork(string input)
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

    private async Task DigitProcessing(string input)
    {
        // RemoveNonDigits step (though input should already be digits only)
        var digitsOnly = await RemoveNonDigits(input);

        // ParseToInt step
        var intResult = await ParseToInt(digitsOnly);

        // IntHandler
        await IntHandler(intResult);
    }

    private async Task NonDigitProcessing(string input)
    {
        // RemoveDigits step
        var nonDigitsOnly = await RemoveDigits(input);

        // AddSpaces step
        var withSpaces = await AddSpaces(nonDigitsOnly);

        // StringHandler
        await StringHandler(withSpaces);
    }

    private async Task<string> RemoveNonDigits(string input)
    {
        return new string(input.Where(char.IsDigit).ToArray());
    }

    private async Task<int> ParseToInt(string input)
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

    private async Task<string> RemoveDigits(string input)
    {
        return new string(input.Where(c => !char.IsDigit(c)).ToArray());
    }

    private async Task<string> AddSpaces(string input)
    {
        return $"  {input}  ";
    }

    private async Task IntHandler(int input)
    {
        _consumer.Consume(input);
    }

    private async Task StringHandler(string input)
    {
        _consumer.Consume(input);
    }
}
