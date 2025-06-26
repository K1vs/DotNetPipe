using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsOneStepFast;

internal class VirtualClass
{
    private readonly Consumer _consumer = new();

    public void Run(int input)
    {
        RunInternal(input);
    }

    protected virtual void RunInternal(int input)
    {
        ConsumeNumber(input);
    }

    protected virtual void ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
