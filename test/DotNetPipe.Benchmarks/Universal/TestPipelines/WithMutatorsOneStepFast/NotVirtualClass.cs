using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsOneStepFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public async ValueTask Run(int input)
    {
        await RunInternal(input);
    }

    protected async ValueTask RunInternal(int input)
    {
        // Apply mutation: add 1 to input
        input += 1;
        await ConsumeNumber(input);
    }

    private async ValueTask ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
