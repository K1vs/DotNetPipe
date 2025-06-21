using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsOneStepFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public async ValueTask Run(int input)
    {
        await RunInternal(input);
    }

    protected virtual async ValueTask RunInternal(int input)
    {
        await ConsumeNumber(input);
    }

    protected virtual async ValueTask ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
