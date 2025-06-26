using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsSwitchFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public void Run(string input)
    {
        RunInternal(input);
    }

    protected virtual void RunInternal(string input)
    {
        // Phase 1: TrimString
        var trimmed = TrimString(input);

        // Phase 2: NumberRangeSwitch
        var result = NumberRangeSwitch(trimmed);

        // Phase 3: TestHandler
        TestHandler(result);
    }

    protected virtual string TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual int NumberRangeSwitch(string input)
    {
        // Try to parse as integer
        if (int.TryParse(input, out var number))
        {
            if (number > 100)
            {
                return MultiplyByThree(number);
            }
            else if (number > 0)
            {
                return AddTwo(number);
            }
            else if (number < 0)
            {
                return MultiplyByTwo(number);
            }
            else // number == 0
            {
                return KeepZero(number);
            }
        }
        else
        {
            // If not a number, use string length
            var stringLength = input.Length;
            return StringLengthProcessing(stringLength);
        }
    }

    protected virtual int MultiplyByThree(int input)
    {
        return input * 3;
    }

    protected virtual int AddTwo(int input)
    {
        return input + 2;
    }

    protected virtual int MultiplyByTwo(int input)
    {
        return input * 2;
    }

    protected virtual int KeepZero(int input)
    {
        return input; // Keep the same value (0)
    }

    protected virtual int StringLengthProcessing(int input)
    {
        return input; // Use string length as-is
    }

    protected virtual void TestHandler(int input)
    {
        _consumer.Consume(input);
    }
}

// Derived class that represents the mutation behavior
internal class VirtualClassWithMutation : VirtualClass
{
    protected override int NumberRangeSwitch(string input)
    {
        // Mutated logic: change the thresholds (50 instead of 100)
        if (int.TryParse(input, out var number))
        {
            if (number > 50) // was 100
            {
                return MultiplyByThree(number);
            }
            else if (number > 0)
            {
                return AddTwo(number);
            }
            else // include 0 in negative processing, was < 0
            {
                return MultiplyByTwo(number);
            }
        }
        else
        {
            // If not a number, use string length
            var stringLength = input.Length;
            return StringLengthProcessing(stringLength);
        }
    }

    protected override int MultiplyByThree(int input)
    {
        // Mutation: add 3 before multiplying by 3
        input += 3;
        return input * 3;
    }

    protected override int StringLengthProcessing(int input)
    {
        // Mutation: add 3 to string length
        return input + 3;
    }
}
