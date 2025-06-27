using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsTwoStepsFast;

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
        var result = await AddConstant(input);
        await ConsumeNumber(result);
    }

    private async Task<int> AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    private async Task ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
