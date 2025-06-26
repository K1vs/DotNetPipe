using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsTwoStepsFast;

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
