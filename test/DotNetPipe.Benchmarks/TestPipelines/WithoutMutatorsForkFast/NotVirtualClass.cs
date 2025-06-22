using BenchmarkDotNet.Engines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsForkFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public async ValueTask Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: DigitContentFork
        await DigitContentFork(trimmed);
    }

    private async ValueTask<string> TrimString(string input)
    {
        return input.Trim();
    }

    private async ValueTask DigitContentFork(string input)
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

    private async ValueTask DigitProcessing(string input)
    {
        // RemoveNonDigits step (though input should already be digits only)
        var digitsOnly = await RemoveNonDigits(input);

        // ParseToInt step
        var intResult = await ParseToInt(digitsOnly);

        // IntHandler
        await IntHandler(intResult);
    }

    private async ValueTask NonDigitProcessing(string input)
    {
        // RemoveDigits step
        var nonDigitsOnly = await RemoveDigits(input);

        // AddSpaces step
        var withSpaces = await AddSpaces(nonDigitsOnly);

        // StringHandler
        await StringHandler(withSpaces);
    }

    private async ValueTask<string> RemoveNonDigits(string input)
    {
        return new string(input.Where(char.IsDigit).ToArray());
    }

    private async ValueTask<int> ParseToInt(string input)
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

    private async ValueTask<string> RemoveDigits(string input)
    {
        return new string(input.Where(c => !char.IsDigit(c)).ToArray());
    }

    private async ValueTask<string> AddSpaces(string input)
    {
        return $"  {input}  ";
    }

    private async ValueTask IntHandler(int input)
    {
        _consumer.Consume(input);
    }

    private async ValueTask StringHandler(string input)
    {
        _consumer.Consume(input);
    }
}
