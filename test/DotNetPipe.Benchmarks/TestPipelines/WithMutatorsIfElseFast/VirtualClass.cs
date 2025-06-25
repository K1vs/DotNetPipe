using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithMutatorsIfElseFast;

internal class VirtualClass
{
    protected const int _multiplier = 2;
    protected const int _constantToAdd = 4;
    protected readonly Consumer _consumer = new();

    public async ValueTask Run(string input)
    {
        await RunInternal(input);
    }

    protected virtual async ValueTask RunInternal(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: CheckIntOrFloat
        var result = await CheckIntOrFloat(trimmed);

        // Phase 3: AddConstant
        var finalResult = await AddConstant(result);

        // Phase 4: TestHandler
        await TestHandler(finalResult);
    }

    protected virtual async ValueTask<string> TrimString(string input)
    {
        return input.Trim();
    }

    protected virtual async ValueTask<int> CheckIntOrFloat(string input)
    {
        // Try to parse as int first
        if (int.TryParse(input, out var intValue))
        {
            // False branch - multiply by multiplier (same as ParseIntOrDefault in the pipeline)
            return await ParseIntOrDefault(intValue);
        }
        else
        {
            // True branch - float processing
            return await FloatProcessing(input);
        }
    }

    protected virtual async ValueTask<int> FloatProcessing(string input)
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

    protected virtual async ValueTask<int> ParseIntOrDefault(int input)
    {
        // If we got here, it means we parsed as int in the false branch
        // So we just pass it through multiplied by multiplier
        return input * _multiplier;
    }

    protected virtual async ValueTask<int> AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    protected virtual async ValueTask TestHandler(int input)
    {
        _consumer.Consume(input);
    }
}

// Derived class that represents the mutation behavior
internal class VirtualClassWithMutation : VirtualClass
{
    protected override async ValueTask<int> CheckIntOrFloat(string input)
    {
        // Mutated logic: swap the branches - what was true becomes false and vice versa
        if (int.TryParse(input, out var intValue))
        {
            // If it's an int, go to float processing (was false branch before)
            // Now int values go to float processing pipeline
            return await FloatProcessing(intValue.ToString());
        }
        else
        {
            // If not an int, go to int processing (was true branch before)
            return await ParseIntOrDefault(0); // Default value since we can't parse
        }
    }

    protected override async ValueTask<int> FloatProcessing(string input)
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

    protected override async ValueTask<int> ParseIntOrDefault(int input)
    {
        // Mutation: multiply by (multiplier + 2) instead of just multiplier
        return input * (_multiplier + 2);
    }

    protected override async ValueTask TestHandler(int input)
    {
        // Mutation: add 1 to the final result
        input += 1;
        _consumer.Consume(input);
    }
}
