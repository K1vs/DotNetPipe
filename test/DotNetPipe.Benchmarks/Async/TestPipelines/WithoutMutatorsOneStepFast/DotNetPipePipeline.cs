using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Async;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsOneStepFast;

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
        var pipeline = Pipelines.CreateAsyncPipeline<int>("SimpleOneStepPipeline")
            .StartWithHandler("ConsumeNumber", async (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();
        return pipeline;
    }

    public async Task Run(int input)
    {
        await _handler(input);
    }
}
