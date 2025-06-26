using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsSwitchFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public async ValueTask Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: NumberRangeSwitch (with mutation)
        var result = await NumberRangeSwitch(trimmed);

        // Phase 3: TestHandler
        await TestHandler(result);
    }

    private async ValueTask<string> TrimString(string input)
    {
        return input.Trim();
    }

    private async ValueTask<int> NumberRangeSwitch(string input)
    {
        // Mutated logic: change the thresholds (50 instead of 100)
        if (int.TryParse(input, out var number))
        {
            if (number > 50) // was 100
            {
                return await MultiplyByThree(number);
            }
            else if (number > 0)
            {
                return await AddTwo(number);
            }
            else // include 0 in negative processing, was < 0
            {
                return await MultiplyByTwo(number);
            }
        }
        else
        {
            // If not a number, use string length
            var stringLength = input.Length;
            return await StringLengthProcessing(stringLength);
        }
    }

    private async ValueTask<int> MultiplyByThree(int input)
    {
        // Mutation: add 3 before multiplying by 3
        input += 3;
        return input * 3;
    }

    private async ValueTask<int> AddTwo(int input)
    {
        return input + 2;
    }

    private async ValueTask<int> MultiplyByTwo(int input)
    {
        return input * 2;
    }

    private async ValueTask<int> StringLengthProcessing(int input)
    {
        // Mutation: add 3 to string length
        return input + 3;
    }

    private async ValueTask TestHandler(int input)
    {
        _consumer.Consume(input);
    }
}
