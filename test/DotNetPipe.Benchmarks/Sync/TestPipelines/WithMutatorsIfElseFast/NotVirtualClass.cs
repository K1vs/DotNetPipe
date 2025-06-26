using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsIfElseFast;

internal class NotVirtualClass
{
    private const int _multiplier = 2;
    private const int _constantToAdd = 4;
    private readonly Consumer _consumer = new();

    public async void Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = TrimString(input);

        // Phase 2: CheckIntOrFloat (with mutation)
        var result = CheckIntOrFloat(trimmed);

        // Phase 3: AddConstant
        var finalResult = AddConstant(result);

        // Phase 4: TestHandler (with mutation)
        TestHandler(finalResult);
    }

    private string TrimString(string input)
    {
        return input.Trim();
    }

    private int CheckIntOrFloat(string input)
    {
        // Mutated logic: swap the branches - what was true becomes false and vice versa
        if (int.TryParse(input, out var intValue))
        {
            // If it's an int, go to float processing (was false branch before)
            // Now int values go to float processing pipeline
            return FloatProcessing(intValue.ToString());
        }
        else
        {
            // If not an int, go to int processing (was true branch before)
            return ParseIntOrDefault(0); // Default value since we can't parse
        }
    }

    private int FloatProcessing(string input)
    {
        // ParseFloat step
        if (double.TryParse(input, out var floatValue))
        {
            // Mutation: add 1 to the input before rounding
            floatValue += 1;
            var rounded = (int)Math.Round(floatValue);
            return rounded;
        }

        // If parsing fails, return 0 (default behavior)
        return 0;
    }

    private int ParseIntOrDefault(int input)
    {
        // Mutation: multiply by (multiplier + 2) instead of just multiplier
        return input * (_multiplier + 2);
    }

    private int AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    private void TestHandler(int input)
    {
        // Mutation: add 1 to the final result
        input += 1;
        _consumer.Consume(input);
    }
}
