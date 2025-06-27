using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsTwoStepsFast;

internal class VirtualClass
{
    private const int _constantToAdd = 10;
    private readonly Consumer _consumer = new();

    public async Task Run(int input)
    {
        await RunInternal(input);
    }

    protected virtual async Task RunInternal(int input)
    {
        var result = await AddConstant(input);
        await ConsumeNumber(result);
    }

    protected virtual async Task<int> AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    protected virtual async Task ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
