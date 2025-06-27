using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsSwitchFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public async Task Run(string input)
    {
        await RunInternal(input);
    }

    protected virtual async Task RunInternal(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: NumberRangeSwitch
        var result = await NumberRangeSwitch(trimmed);

        // Phase 3: TestHandler
        await TestHandler(result);
    }

    protected virtual async Task<string> TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual async Task<int> NumberRangeSwitch(string input)
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
            return await StringLengthProcessing(stringLength);
        }
    }

    protected virtual async Task<int> MultiplyByThree(int input)
    {
        return input * 3;
    }

    protected virtual async Task<int> AddTwo(int input)
    {
        return input + 2;
    }

    protected virtual async Task<int> MultiplyByTwo(int input)
    {
        return input * 2;
    }

    protected virtual async Task<int> KeepZero(int input)
    {
        return input; // Keep the same value (0)
    }

    protected virtual async Task<int> StringLengthProcessing(int input)
    {
        return input; // Use string length as-is
    }

    protected virtual async Task TestHandler(int input)
    {
        _consumer.Consume(input);
    }
}

// Derived class that represents the mutation behavior
internal class VirtualClassWithMutation : VirtualClass
{
    protected override async Task<int> NumberRangeSwitch(string input)
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

    protected override async Task<int> MultiplyByThree(int input)
    {
        // Mutation: add 3 before multiplying by 3
        input += 3;
        return input * 3;
    }

    protected override async Task<int> StringLengthProcessing(int input)
    {
        // Mutation: add 3 to string length
        return input + 3;
    }
}
