using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithoutMutatorsSwitchFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public async ValueTask Run(string input)
    {
        await RunInternal(input);
    }

    protected virtual async ValueTask RunInternal(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: NumberRangeSwitch
        var result = await NumberRangeSwitch(trimmed);

        // Phase 3: TestHandler
        await TestHandler(result);
    }

    protected virtual async ValueTask<string> TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual async ValueTask<int> NumberRangeSwitch(string input)
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

    protected virtual async ValueTask<int> MultiplyByThree(int input)
    {
        return input * 3;
    }

    protected virtual async ValueTask<int> AddTwo(int input)
    {
        return input + 2;
    }

    protected virtual async ValueTask<int> MultiplyByTwo(int input)
    {
        return input * 2;
    }

    protected virtual async ValueTask<int> KeepZero(int input)
    {
        return input; // Keep the same value (0)
    }

    protected virtual async ValueTask<int> StringLengthIdentity(int input)
    {
        return input; // Use string length as-is
    }

    protected virtual async ValueTask TestHandler(int input)
    {
        _consumer.Consume(input);
    }
}
