using BenchmarkDotNet.Engines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsMultiForkFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public virtual void Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = TrimString(input);

        // Phase 2: ClassifyStringContent
        ClassifyStringContent(trimmed);
    }

    protected virtual string TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual void ClassifyStringContent(string input)
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

    protected virtual void DigitBranch(string input)
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

    protected virtual void LetterBranch(string input)
    {
        // AddSpaces
        var withSpaces = $"  {input}  ";

        // StringHandler
        StringHandler(withSpaces);
    }

    protected virtual void SpecialCharBranch(string input)
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

    protected virtual void DefaultBranch(string input)
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

    protected virtual void IntHandler(int input)
    {
        _consumer.Consume(input);
    }

    protected virtual void StringHandler(string input)
    {
        _consumer.Consume(input);
    }

    protected virtual void CharArrayHandler(char[] input)
    {
        _consumer.Consume(input);
    }
}
