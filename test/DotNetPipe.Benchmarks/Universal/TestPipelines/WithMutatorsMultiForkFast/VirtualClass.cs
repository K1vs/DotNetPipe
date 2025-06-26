using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Engines;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsMultiForkFast;

internal class VirtualClass
{
    protected readonly Consumer Consumer = new();

    public virtual async ValueTask Run(string input)
    {
        // TrimString
        var trimmed = TrimString(input);

        // ClassifyStringContent
        await ClassifyStringContent(trimmed);
    }

    protected virtual string TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual async ValueTask ClassifyStringContent(string input)
    {
        var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
        var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
        var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

        if (containsOnlyDigits)
        {
            await ProcessDigits(input);
        }
        else if (containsOnlyLetters)
        {
            await ProcessLetters(input);
        }
        else if (containsOnlySpecialChars)
        {
            await ProcessSpecialChars(input);
        }
        else
        {
            // Mixed content -> default pipeline
            var charArray = input.ToCharArray();
            await ProcessDefault(charArray);
        }
    }

    protected virtual async ValueTask ProcessDigits(string input)
    {
        var number = int.TryParse(input, out var parsed) ? parsed : 0;
        var result = number + 10; // AddConstant
        Consumer.Consume(result);
        await ValueTask.CompletedTask;
    }

    protected virtual async ValueTask ProcessLetters(string input)
    {
        var withSpaces = $"  {input}  "; // AddSpaces
        Consumer.Consume(withSpaces);
        await ValueTask.CompletedTask;
    }

    protected virtual async ValueTask ProcessSpecialChars(string input)
    {
        var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
        var charArray = noWhitespace.ToCharArray();
        var uniqueChars = charArray.Distinct().ToArray();
        Consumer.Consume(uniqueChars);
        await ValueTask.CompletedTask;
    }

    protected virtual async ValueTask ProcessDefault(char[] input)
    {
        var digitCount = input.Count(char.IsDigit);
        var letterCount = input.Count(char.IsLetter);
        var ratio = letterCount > 0 ? digitCount / letterCount : digitCount;
        Consumer.Consume(ratio);
        await ValueTask.CompletedTask;
    }
}

internal class VirtualClassWithMutation : VirtualClass
{
    // Override ClassifyStringContent to use mutated logic
    protected override async ValueTask ClassifyStringContent(string input)
    {
        var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

        if (containsOnlyDigits)
        {
            // Only digits go to digit branch
            await ProcessDigits(input);
        }
        else
        {
            // Everything else goes to special char branch
            await ProcessSpecialChars(input);
        }
    }

    // Override ProcessDigits with mutated AddConstant
    protected override async ValueTask ProcessDigits(string input)
    {
        var number = int.TryParse(input, out var parsed) ? parsed : 0;
        var result = number + 10 + 5; // AddConstant with mutation (+5 more)
        Consumer.Consume(result);
        await ValueTask.CompletedTask;
    }

    // Override ProcessLetters with mutated AddSpaces
    protected override async ValueTask ProcessLetters(string input)
    {
        var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
        Consumer.Consume(withAsterisks);
        await ValueTask.CompletedTask;
    }

    // Override ProcessSpecialChars with mutated ConvertToCharArray
    protected override async ValueTask ProcessSpecialChars(string input)
    {
        var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
        var inputWithUnderscore = noWhitespace + "_"; // Add underscore before converting
        var charArray = inputWithUnderscore.ToCharArray();
        var uniqueChars = charArray.Distinct().ToArray();
        Consumer.Consume(uniqueChars);
        await ValueTask.CompletedTask;
    }

    // Override ProcessDefault with mutated CalculateRatio
    protected override async ValueTask ProcessDefault(char[] input)
    {
        var digitCount = input.Count(char.IsDigit);
        var letterCount = input.Count(char.IsLetter);
        var ratio = letterCount > 0 ? digitCount / letterCount : digitCount;
        var mutatedRatio = ratio + 2; // Add 2 to the calculated ratio
        Consumer.Consume(mutatedRatio);
        await ValueTask.CompletedTask;
    }
}
