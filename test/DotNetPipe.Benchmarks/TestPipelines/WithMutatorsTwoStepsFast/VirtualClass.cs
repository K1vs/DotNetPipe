using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithMutatorsTwoStepsFast;

internal class VirtualClass
{
    private const int _constantToAdd = 10;
    private readonly Consumer _consumer = new();

    public async ValueTask Run(int input)
    {
        await RunInternal(input);
    }

    protected virtual async ValueTask RunInternal(int input)
    {
        var result = await AddConstant(input);
        await ConsumeNumber(result);
    }

    protected virtual async ValueTask<int> AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    protected virtual async ValueTask ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}

// Derived class that represents the mutation behavior
internal class VirtualClassWithMutation : VirtualClass
{
    protected override async ValueTask RunInternal(int input)
    {
        // Apply first mutation: multiply by 2
        input *= 2;
        var result = await AddConstant(input);
        // Apply second mutation: add 1
        result += 1;
        await ConsumeNumber(result);
    }
}
