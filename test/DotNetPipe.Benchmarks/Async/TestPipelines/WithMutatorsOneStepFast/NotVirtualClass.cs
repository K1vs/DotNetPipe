using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsOneStepFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public async Task Run(int input)
    {
        await RunInternal(input);
    }

    protected async Task RunInternal(int input)
    {
        // Apply mutation: add 1 to input
        input += 1;
        await ConsumeNumber(input);
    }

    private async Task ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
