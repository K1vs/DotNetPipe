using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsOneStepFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public async Task Run(int input)
    {
        await RunInternal(input);
    }

    protected virtual async Task RunInternal(int input)
    {
        await ConsumeNumber(input);
    }

    protected virtual async Task ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
