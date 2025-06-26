using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsOneStepFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public void Run(int input)
    {
        RunInternal(input);
    }

    protected void RunInternal(int input)
    {
        // Apply mutation: add 1 to input
        input += 1;
        ConsumeNumber(input);
    }

    private void ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
