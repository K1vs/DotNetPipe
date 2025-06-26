using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Engines;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsMultiForkFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public async ValueTask Run(string input)
    {
        // TrimString
        var trimmed = input.Trim();

        // ClassifyStringContent with mutation: only digits go to digit branch, everything else to special char
        var containsOnlyDigits = !string.IsNullOrEmpty(trimmed) && trimmed.All(char.IsDigit);

        if (containsOnlyDigits)
        {
            // DigitProcessingPipeline
            var number = int.TryParse(trimmed, out var parsed) ? parsed : 0;
            var digitResult = number + 10 + 5; // AddConstant with mutation (+5 more)
        }
        else
        {
            // SpecialCharProcessingPipeline (mutated logic - everything else goes here)
            var noWhitespace = new string(trimmed.Where(c => !char.IsWhiteSpace(c)).ToArray());
            var inputWithUnderscore = noWhitespace + "_"; // ConvertToCharArray with mutation
            var charArray = inputWithUnderscore.ToCharArray();
            var uniqueChars = charArray.Distinct().ToArray(); // RemoveDuplicates
        }

        _consumer.Consume(trimmed);

        await ValueTask.CompletedTask;
    }
}
