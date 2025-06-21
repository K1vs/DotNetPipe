using BenchmarkDotNet.Engines;
using System;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsComplexFast;

internal class VirtualClass
{
    private const int _multiplier = 2;
    private const int _constantToAdd = 4;
    private readonly Consumer _consumer = new();

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
