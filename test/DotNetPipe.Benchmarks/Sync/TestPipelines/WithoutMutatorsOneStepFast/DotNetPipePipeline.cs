using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Sync;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsOneStepFast;

internal class DotNetPipePipeline
{
    private readonly Handler<int> _handler;
    private readonly Consumer _consumer = new();

    public DotNetPipePipeline()
    {
        _handler = CreateHandler();
    }

    private Handler<int> CreateHandler()
    {
        var pipeline = Pipelines.CreateSyncPipeline<int>("SimpleOneStepPipeline")
            .StartWithHandler("ConsumeNumber", (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();
        return pipeline;
    }

    public void Run(int input)
    {
        _handler(input);
    }
}
