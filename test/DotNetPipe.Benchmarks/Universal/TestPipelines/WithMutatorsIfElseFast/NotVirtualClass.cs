using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsIfElseFast;

internal class NotVirtualClass
{
    private const int _multiplier = 2;
    private const int _constantToAdd = 4;
    private readonly Consumer _consumer = new();

    public async ValueTask Run(string input)
    {
        // Phase 1: TrimString
        var trimmed = await TrimString(input);

        // Phase 2: CheckIntOrFloat (with mutation)
        var result = await CheckIntOrFloat(trimmed);

        // Phase 3: AddConstant
        var finalResult = await AddConstant(result);

        // Phase 4: TestHandler (with mutation)
        await TestHandler(finalResult);
    }

    private async ValueTask<string> TrimString(string input)
    {
        return input.Trim();
    }

    private async ValueTask<int> CheckIntOrFloat(string input)
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

    private async ValueTask<int> FloatProcessing(string input)
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

    private async ValueTask<int> ParseIntOrDefault(int input)
    {
        // Mutation: multiply by (multiplier + 2) instead of just multiplier
        return input * (_multiplier + 2);
    }

    private async ValueTask<int> AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    private async ValueTask TestHandler(int input)
    {
        // Mutation: add 1 to the final result
        input += 1;
        _consumer.Consume(input);
    }
}
