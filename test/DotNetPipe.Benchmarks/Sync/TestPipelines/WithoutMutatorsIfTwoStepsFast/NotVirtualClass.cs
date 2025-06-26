using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsIfTwoStepsFast;

internal class NotVirtualClass
{
    private const int _multiplier = -2;
    private readonly Consumer _consumer = new();

    public void Run(int input)
    {
        RunInternal(input);
    }

    protected void RunInternal(int input)
    {
        int result;
        if (input < 0)
        {
            result = MultiplyByConstant(input);
        }
        else
        {
            result = input;
        }
        ConsumeNumber(result);
    }

    private int MultiplyByConstant(int input)
    {
        return input * _multiplier;
    }

    private void ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
