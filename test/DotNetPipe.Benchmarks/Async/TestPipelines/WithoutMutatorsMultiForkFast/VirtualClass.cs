using BenchmarkDotNet.Engines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsMultiForkFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public virtual async Task Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: ClassifyStringContent
        await ClassifyStringContent(trimmed);
    }

    protected virtual async Task<string> TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual async Task ClassifyStringContent(string input)
    {
        var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
        var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
        var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

        if (containsOnlyDigits)
        {
            await DigitBranch(input);
        }
        else if (containsOnlyLetters)
        {
            await LetterBranch(input);
        }
        else if (containsOnlySpecialChars)
        {
            await SpecialCharBranch(input);
        }
        else
        {
            await DefaultBranch(input);
        }
    }

    protected virtual async Task DigitBranch(string input)
    {
        // ParseStringToInt
        int number;
        if (int.TryParse(input, out number))
        {
            // Do nothing, keep number
        }
        else
        {
            number = 0; // Default value if parsing fails
        }

        // AddConstant
        var result = number + 10; // Add constant 10

        // IntHandler
        await IntHandler(result);
    }

    protected virtual async Task LetterBranch(string input)
    {
        // AddSpaces
        var withSpaces = $"  {input}  ";

        // StringHandler
        await StringHandler(withSpaces);
    }

    protected virtual async Task SpecialCharBranch(string input)
    {
        // RemoveWhitespace
        var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());

        // ConvertToCharArray
        var charArray = noWhitespace.ToCharArray();

        // RemoveDuplicates
        var uniqueChars = charArray.Distinct().ToArray();

        // CharArrayHandler
        await CharArrayHandler(uniqueChars);
    }

    protected virtual async Task DefaultBranch(string input)
    {
        var charArray = input.ToCharArray();

        // CountDigitsAndLetters
        var digitCount = charArray.Count(char.IsDigit);
        var letterCount = charArray.Count(char.IsLetter);

        // CalculateRatio
        var ratio = letterCount > 0 ? digitCount / letterCount : digitCount;

        // IntHandler
        await IntHandler(ratio);
    }

    protected virtual async Task IntHandler(int input)
    {
        _consumer.Consume(input);
    }

    protected virtual async Task StringHandler(string input)
    {
        _consumer.Consume(input);
    }

    protected virtual async Task CharArrayHandler(char[] input)
    {
        _consumer.Consume(input);
    }
}
