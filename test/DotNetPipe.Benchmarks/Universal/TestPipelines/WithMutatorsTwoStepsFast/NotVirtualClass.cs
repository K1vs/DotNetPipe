using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsTwoStepsFast;

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
        // Apply first mutation: multiply by 2
        input *= 2;
        var result = await AddConstant(input);
        // Apply second mutation: add 1
        result += 1;
        await ConsumeNumber(result);
    }

    protected async ValueTask<int> AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    private async ValueTask ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
