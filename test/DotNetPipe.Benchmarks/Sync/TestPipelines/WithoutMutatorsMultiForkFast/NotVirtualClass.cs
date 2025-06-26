using BenchmarkDotNet.Engines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsMultiForkFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public void Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = TrimString(input);

        // Phase 2: ClassifyStringContent
        ClassifyStringContent(trimmed);
    }

    private string TrimString(string input)
    {
        return input.Trim();
    }

    private void ClassifyStringContent(string input)
    {
        var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
        var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
        var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

        if (containsOnlyDigits)
        {
            DigitBranch(input);
        }
        else if (containsOnlyLetters)
        {
            LetterBranch(input);
        }
        else if (containsOnlySpecialChars)
        {
            SpecialCharBranch(input);
        }
        else
        {
            DefaultBranch(input);
        }
    }

    private void DigitBranch(string input)
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
        IntHandler(result);
    }

    private async void LetterBranch(string input)
    {
        // AddSpaces
        var withSpaces = $"  {input}  ";

        // StringHandler
        StringHandler(withSpaces);
    }

    private void SpecialCharBranch(string input)
    {
        // RemoveWhitespace
        var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());

        // ConvertToCharArray
        var charArray = noWhitespace.ToCharArray();

        // RemoveDuplicates
        var uniqueChars = charArray.Distinct().ToArray();

        // CharArrayHandler
        CharArrayHandler(uniqueChars);
    }

    private void DefaultBranch(string input)
    {
        var charArray = input.ToCharArray();

        // CountDigitsAndLetters
        var digitCount = charArray.Count(char.IsDigit);
        var letterCount = charArray.Count(char.IsLetter);

        // CalculateRatio
        var ratio = letterCount > 0 ? digitCount / letterCount : digitCount;

        // IntHandler
        IntHandler(ratio);
    }

    private void IntHandler(int input)
    {
        _consumer.Consume(input);
    }

    private void StringHandler(string input)
    {
        _consumer.Consume(input);
    }

    private void CharArrayHandler(char[] input)
    {
        _consumer.Consume(input);
    }
}
