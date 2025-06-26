using BenchmarkDotNet.Engines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsForkFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public void Run(string input)
    {
        RunInternal(input);
    }

    protected virtual void RunInternal(string input)
    {
        // Phase 1: TrimString
        var trimmed = TrimString(input);

        // Phase 2: DigitContentFork
        DigitContentFork(trimmed);
    }

    protected virtual string TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual void DigitContentFork(string input)
    {
        // Check if string contains only digits (after trimming)
        var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

        if (containsOnlyDigits)
        {
            DigitProcessing(input);
        }
        else
        {
            NonDigitProcessing(input);
        }
    }

    protected virtual void DigitProcessing(string input)
    {
        // RemoveNonDigits step (though input should already be digits only)
        var digitsOnly = RemoveNonDigits(input);

        // ParseToInt step
        var intResult = ParseToInt(digitsOnly);

        // IntHandler
        IntHandler(intResult);
    }

    protected virtual void NonDigitProcessing(string input)
    {
        // RemoveDigits step
        var nonDigitsOnly = RemoveDigits(input);

        // AddSpaces step
        var withSpaces = AddSpaces(nonDigitsOnly);

        // StringHandler
        StringHandler(withSpaces);
    }

    protected virtual string RemoveNonDigits(string input)
    {
        return new string(input.Where(char.IsDigit).ToArray());
    }

    protected virtual int ParseToInt(string input)
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

    protected virtual string RemoveDigits(string input)
    {
        return new string(input.Where(c => !char.IsDigit(c)).ToArray());
    }

    protected virtual string AddSpaces(string input)
    {
        return $"  {input}  ";
    }

    protected virtual void IntHandler(int input)
    {
        _consumer.Consume(input);
    }

    protected virtual void StringHandler(string input)
    {
        _consumer.Consume(input);
    }
}
