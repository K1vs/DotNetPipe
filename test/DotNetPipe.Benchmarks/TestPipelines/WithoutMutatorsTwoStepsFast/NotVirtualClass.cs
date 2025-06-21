using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsTwoStepsFast;

internal class NotVirtualClass
{
    private const int _constantToAdd = 10;
    private readonly Consumer _consumer = new();

    public async ValueTask Run(int input)
    {
        await RunInternal(input);
    }

    protected async ValueTask RunInternal(int input)
    {
        var result = await AddConstant(input);
        await ConsumeNumber(result);
    }

    private async ValueTask<int> AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    private async ValueTask ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
