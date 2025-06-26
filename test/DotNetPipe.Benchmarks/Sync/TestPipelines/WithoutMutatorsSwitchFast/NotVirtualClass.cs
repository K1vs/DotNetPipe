using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsSwitchFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public void Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = TrimString(input);

        // Phase 2: NumberRangeSwitch
        var result = NumberRangeSwitch(trimmed);

        // Phase 3: TestHandler
        TestHandler(result);
    }

    private string TrimString(string input)
    {
        return input.Trim();
    }

    private int NumberRangeSwitch(string input)
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
            return StringLengthIdentity(stringLength);
        }
    }

    private int MultiplyByThree(int input)
    {
        return input * 3;
    }

    private int AddTwo(int input)
    {
        return input + 2;
    }

    private int MultiplyByTwo(int input)
    {
        return input * 2;
    }

    private int KeepZero(int input)
    {
        return input; // Keep the same value (0)
    }

    private int StringLengthIdentity(int input)
    {
        return input; // Use string length as-is
    }

    private void TestHandler(int input)
    {
        _consumer.Consume(input);
    }
}
