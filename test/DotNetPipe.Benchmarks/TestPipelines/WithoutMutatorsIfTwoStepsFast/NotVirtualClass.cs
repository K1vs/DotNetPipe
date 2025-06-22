using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsIfTwoStepsFast;

internal class NotVirtualClass
{
    private const int _multiplier = -2;
    private readonly Consumer _consumer = new();

    public async ValueTask Run(int input)
    {
        await RunInternal(input);
    }

    protected async ValueTask RunInternal(int input)
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

    private async ValueTask<int> MultiplyByConstant(int input)
    {
        return input * _multiplier;
    }

    private async ValueTask ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
