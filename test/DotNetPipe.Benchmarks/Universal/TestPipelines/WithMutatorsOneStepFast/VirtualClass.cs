using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsOneStepFast;

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

// Derived class that represents the mutation behavior
internal class VirtualClassWithMutation : VirtualClass
{
    protected override async ValueTask RunInternal(int input)
    {
        // Apply mutation: add 1 to input
        input += 1;
        await base.RunInternal(input);
    }
}
