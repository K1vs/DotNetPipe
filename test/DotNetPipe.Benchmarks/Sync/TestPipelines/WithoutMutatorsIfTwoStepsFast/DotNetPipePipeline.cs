using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Sync;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsIfTwoStepsFast;

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
        var pipeline = Pipelines.CreateSyncPipeline<int>("IfTwoStepPipeline")
            .StartWithIf<int, int>("CheckNegative", (input, conditionalNext, next) =>
            {
                if (input < 0)
                {
                    // Negative number - go to conditional branch for multiplication
                    conditionalNext(input);
                }
                else
                {
                    // Non-negative number - continue with main pipeline
                    next(input);
                }
            },
            // Conditional branch - multiply negative number by constant
            space => space.CreatePipeline<int>("MultiplyNegative")
                .StartWithLinear<int>("MultiplyByConstant", (input, next) =>
                {
                    var result = input * _multiplier;
                    next(result);
                })
                .BuildOpenPipeline())
            .HandleWith("ConsumeNumber", (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();
        return pipeline;
    }

    public void Run(int input)
    {
        _handler(input);
    }
}
