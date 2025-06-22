using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Universal;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsIfTwoStepsFast;

internal class DotNetPipePipeline
{
    private const int _multiplier = -2;
    private readonly Handler<int> _handler;
    private readonly Consumer _consumer = new();

    public DotNetPipePipeline()
    {
        _handler = CreateHandler();
    }

    private Handler<int> CreateHandler()
    {
        var pipeline = Pipelines.CreatePipeline<int>("IfTwoStepPipeline")
            .StartWithIf<int, int>("CheckNegative", async (input, conditionalNext, next) =>
            {
                if (input < 0)
                {
                    // Negative number - go to conditional branch for multiplication
                    await conditionalNext(input);
                }
                else
                {
                    // Non-negative number - continue with main pipeline
                    await next(input);
                }
            },
            // Conditional branch - multiply negative number by constant
            space => space.CreatePipeline<int>("MultiplyNegative")
                .StartWithLinear<int>("MultiplyByConstant", async (input, next) =>
                {
                    var result = input * _multiplier;
                    await next(result);
                })
                .BuildOpenPipeline())
            .HandleWith("ConsumeNumber", async (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();
        return pipeline;
    }

    public async ValueTask Run(int input)
    {
        await _handler(input);
    }
}
