using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsTwoStepsFast;

internal class VirtualClass
{
    private const int _constantToAdd = 10;
    private readonly Consumer _consumer = new();

    public void Run(int input)
    {
        RunInternal(input);
    }

    protected virtual void RunInternal(int input)
    {
        var result = AddConstant(input);
        ConsumeNumber(result);
    }

    protected virtual int AddConstant(int input)
    {
        return input + _constantToAdd;
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
        // Apply first mutation: multiply by 2
        input *= 2;
        var result = AddConstant(input);
        // Apply second mutation: add 1
        result += 1;
        ConsumeNumber(result);
    }
}
