using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsOneStepFast;

internal class NotVirtualClass
{
    private readonly Consumer _consumer = new();

    public void Run(int input)
    {
        RunInternal(input);
    }

    protected void RunInternal(int input)
    {
        ConsumeNumber(input);
    }

    private void ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
