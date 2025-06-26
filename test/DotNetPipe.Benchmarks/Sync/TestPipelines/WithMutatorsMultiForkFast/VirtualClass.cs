using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Engines;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsMultiForkFast;

internal class VirtualClass
{
    protected readonly Consumer Consumer = new();

    public virtual void Run(string input)
    {
        // TrimString
        var trimmed = TrimString(input);

        // ClassifyStringContent
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
            ProcessDigits(input);
        }
        else if (containsOnlyLetters)
        {
            ProcessLetters(input);
        }
        else if (containsOnlySpecialChars)
        {
            ProcessSpecialChars(input);
        }
        else
        {
            // Mixed content -> default pipeline
            var charArray = input.ToCharArray();
            ProcessDefault(charArray);
        }
    }

    protected virtual void ProcessDigits(string input)
    {
        var number = int.TryParse(input, out var parsed) ? parsed : 0;
        var result = number + 10; // AddConstant
        Consumer.Consume(result);
    }

    protected virtual void ProcessLetters(string input)
    {
        var withSpaces = $"  {input}  "; // AddSpaces
        Consumer.Consume(withSpaces);
    }

    protected virtual void ProcessSpecialChars(string input)
    {
        var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
        var charArray = noWhitespace.ToCharArray();
        var uniqueChars = charArray.Distinct().ToArray();
        Consumer.Consume(uniqueChars);
    }

    protected virtual void ProcessDefault(char[] input)
    {
        var digitCount = input.Count(char.IsDigit);
        var letterCount = input.Count(char.IsLetter);
        var ratio = letterCount > 0 ? digitCount / letterCount : digitCount;
        Consumer.Consume(ratio);
    }
}

internal class VirtualClassWithMutation : VirtualClass
{
    // Override ClassifyStringContent to use mutated logic
    protected override void ClassifyStringContent(string input)
    {
        var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

        if (containsOnlyDigits)
        {
            // Only digits go to digit branch
            ProcessDigits(input);
        }
        else
        {
            // Everything else goes to special char branch
            ProcessSpecialChars(input);
        }
    }

    // Override ProcessDigits with mutated AddConstant
    protected override void ProcessDigits(string input)
    {
        var number = int.TryParse(input, out var parsed) ? parsed : 0;
        var result = number + 10 + 5; // AddConstant with mutation (+5 more)
        Consumer.Consume(result);
    }

    // Override ProcessLetters with mutated AddSpaces
    protected override void ProcessLetters(string input)
    {
        var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
        Consumer.Consume(withAsterisks);
    }

    // Override ProcessSpecialChars with mutated ConvertToCharArray
    protected override void ProcessSpecialChars(string input)
    {
        var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
        var inputWithUnderscore = noWhitespace + "_"; // Add underscore before converting
        var charArray = inputWithUnderscore.ToCharArray();
        var uniqueChars = charArray.Distinct().ToArray();
        Consumer.Consume(uniqueChars);
    }

    // Override ProcessDefault with mutated CalculateRatio
    protected override void ProcessDefault(char[] input)
    {
        var digitCount = input.Count(char.IsDigit);
        var letterCount = input.Count(char.IsLetter);
        var ratio = letterCount > 0 ? digitCount / letterCount : digitCount;
        var mutatedRatio = ratio + 2; // Add 2 to the calculated ratio
        Consumer.Consume(mutatedRatio);
    }
}
