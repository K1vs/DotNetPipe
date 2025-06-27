using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsOneStepFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public async Task Run(int input)
    {
        await RunInternal(input);
    }

    protected async Task RunInternal(int input)
    {
        await ConsumeNumber(input);
    }

    private async Task ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
