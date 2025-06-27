using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsTwoStepsFast;

internal class NotVirtualClass
{
    private const int _constantToAdd = 10;
    private readonly Consumer _consumer = new();

    public async Task Run(int input)
    {
        await RunInternal(input);
    }

    protected async Task RunInternal(int input)
    {
        // Apply first mutation: multiply by 2
        input *= 2;
        var result = await AddConstant(input);
        // Apply second mutation: add 1
        result += 1;
        await ConsumeNumber(result);
    }

    protected async Task<int> AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    private async Task ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
