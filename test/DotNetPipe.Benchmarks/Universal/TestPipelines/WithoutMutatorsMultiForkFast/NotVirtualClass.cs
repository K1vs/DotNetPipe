using BenchmarkDotNet.Engines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithoutMutatorsMultiForkFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public async ValueTask Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: ClassifyStringContent
        await ClassifyStringContent(trimmed);
    }

    private async ValueTask<string> TrimString(string input)
    {
        return input.Trim();
    }

    private async ValueTask ClassifyStringContent(string input)
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

    private async ValueTask DigitBranch(string input)
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

    private async ValueTask LetterBranch(string input)
    {
        // AddSpaces
        var withSpaces = $"  {input}  ";

        // StringHandler
        await StringHandler(withSpaces);
    }

    private async ValueTask SpecialCharBranch(string input)
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

    private async ValueTask DefaultBranch(string input)
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

    private async ValueTask IntHandler(int input)
    {
        _consumer.Consume(input);
    }

    private async ValueTask StringHandler(string input)
    {
        _consumer.Consume(input);
    }

    private async ValueTask CharArrayHandler(char[] input)
    {
        _consumer.Consume(input);
    }
}
