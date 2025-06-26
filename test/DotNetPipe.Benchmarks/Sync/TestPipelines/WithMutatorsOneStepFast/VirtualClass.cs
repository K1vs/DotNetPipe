using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsOneStepFast;

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

// Derived class that represents the mutation behavior
internal class VirtualClassWithMutation : VirtualClass
{
    protected override void RunInternal(int input)
    {
        // Apply mutation: add 1 to input
        input += 1;
        base.RunInternal(input);
    }
}
