using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsIfElseFast;

internal class VirtualClass
{
    protected const int _multiplier = 2;
    protected const int _constantToAdd = 4;
    protected readonly Consumer _consumer = new();

    public void Run(string input)
    {
        RunInternal(input);
    }

    protected virtual void RunInternal(string input)
    {
        // Phase 1: TrimString
        var trimmed = TrimString(input);

        // Phase 2: CheckIntOrFloat
        var result = CheckIntOrFloat(trimmed);

        // Phase 3: AddConstant
        var finalResult = AddConstant(result);

        // Phase 4: TestHandler
        TestHandler(finalResult);
    }

    protected virtual string TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual int CheckIntOrFloat(string input)
    {
        // Try to parse as int first
        if (int.TryParse(input, out var intValue))
        {
            // False branch - multiply by multiplier (same as ParseIntOrDefault in the pipeline)
            return ParseIntOrDefault(intValue);
        }
        else
        {
            // True branch - float processing
            return FloatProcessing(input);
        }
    }

    protected virtual int FloatProcessing(string input)
    {
        // ParseFloat step
        if (double.TryParse(input, out var floatValue))
        {
            // RoundToInt step
            var rounded = (int)Math.Round(floatValue);
            return rounded;
        }

        // If parsing fails, return 0 (default behavior)
        return 0;
    }

    protected virtual int ParseIntOrDefault(int input)
    {
        // If we got here, it means we parsed as int in the false branch
        // So we just pass it through multiplied by multiplier
        return input * _multiplier;
    }

    protected virtual int AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    protected virtual void TestHandler(int input)
    {
        _consumer.Consume(input);
    }
}

// Derived class that represents the mutation behavior
internal class VirtualClassWithMutation : VirtualClass
{
    protected override int CheckIntOrFloat(string input)
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

    protected override int FloatProcessing(string input)
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

    protected override int ParseIntOrDefault(int input)
    {
        // Mutation: multiply by (multiplier + 2) instead of just multiplier
        return input * (_multiplier + 2);
    }

    protected override void TestHandler(int input)
    {
        // Mutation: add 1 to the final result
        input += 1;
        _consumer.Consume(input);
    }
}
