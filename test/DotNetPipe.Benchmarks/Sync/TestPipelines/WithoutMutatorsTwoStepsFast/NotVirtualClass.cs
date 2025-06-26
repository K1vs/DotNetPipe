using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsTwoStepsFast;

internal class NotVirtualClass
{
    private const int _constantToAdd = 10;
    private readonly Consumer _consumer = new();

    public void Run(int input)
    {
        RunInternal(input);
    }

    protected void RunInternal(int input)
    {
        var result = AddConstant(input);
        ConsumeNumber(result);
    }

    private int AddConstant(int input)
    {
        return input + _constantToAdd;
    }

    private void ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
