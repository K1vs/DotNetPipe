using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsOneStepFast;

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

// Derived class that represents the mutation behavior
internal class VirtualClassWithMutation : VirtualClass
{
    protected override async Task RunInternal(int input)
    {
        // Apply mutation: add 1 to input
        input += 1;
        await base.RunInternal(input);
    }
}
