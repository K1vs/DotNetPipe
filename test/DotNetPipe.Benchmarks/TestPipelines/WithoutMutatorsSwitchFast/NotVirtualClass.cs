using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsSwitchFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public async ValueTask Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: NumberRangeSwitch
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
        // Try to parse as integer
        if (int.TryParse(input, out var number))
        {
            if (number > 100)
            {
                return await MultiplyByThree(number);
            }
            else if (number > 0)
            {
                return await AddTwo(number);
            }
            else if (number < 0)
            {
                return await MultiplyByTwo(number);
            }
            else // number == 0
            {
                return await KeepZero(number);
            }
        }
        else
        {
            // If not a number, use string length
            var stringLength = input.Length;
            return await StringLengthIdentity(stringLength);
        }
    }

    private async ValueTask<int> MultiplyByThree(int input)
    {
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

    private async ValueTask<int> KeepZero(int input)
    {
        return input; // Keep the same value (0)
    }

    private async ValueTask<int> StringLengthIdentity(int input)
    {
        return input; // Use string length as-is
    }

    private async ValueTask TestHandler(int input)
    {
        _consumer.Consume(input);
    }
}
