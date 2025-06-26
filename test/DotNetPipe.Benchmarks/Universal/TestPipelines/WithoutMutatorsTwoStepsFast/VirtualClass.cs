using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithoutMutatorsTwoStepsFast;

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
