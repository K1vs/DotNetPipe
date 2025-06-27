using BenchmarkDotNet.Engines;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsIfTwoStepsFast;

internal class NotVirtualClass
{
    private const int _multiplier = -2;
    private readonly Consumer _consumer = new();

    public async Task Run(int input)
    {
        await RunInternal(input);
    }

    protected async Task RunInternal(int input)
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

    private async Task<int> MultiplyByConstant(int input)
    {
        return input * _multiplier;
    }

    private async Task ConsumeNumber(int input)
    {
        _consumer.Consume(input);
    }
}
