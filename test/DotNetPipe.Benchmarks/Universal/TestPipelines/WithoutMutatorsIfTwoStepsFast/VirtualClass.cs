using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithoutMutatorsIfTwoStepsFast;

internal class VirtualClass
{
    private const int _multiplier = -2;
    private readonly Consumer _consumer = new();

    public async ValueTask Run(int input)
    {
        await RunInternal(input);
    }

    protected virtual async ValueTask RunInternal(int input)
    {
        int result;
        if (input < 0)
        {
            result = await MultiplyByConstant(input);
        }
        else
        {
            result = input;
        }
        await ConsumeNumber(result);
    }

    protected virtual async ValueTask<int> MultiplyByConstant(int input)
    {
        return input * _multiplier;
    }

    protected virtual async ValueTask ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
