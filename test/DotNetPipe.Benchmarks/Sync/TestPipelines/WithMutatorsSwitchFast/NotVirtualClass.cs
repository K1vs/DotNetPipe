using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsSwitchFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public void Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = TrimString(input);

        // Phase 2: NumberRangeSwitch (with mutation)
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

    private int MultiplyByThree(int input)
    {
        // Mutation: add 3 before multiplying by 3
        input += 3;
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

    private int StringLengthProcessing(int input)
    {
        // Mutation: add 3 to string length
        return input + 3;
    }

    private void TestHandler(int input)
    {
        _consumer.Consume(input);
    }
}
