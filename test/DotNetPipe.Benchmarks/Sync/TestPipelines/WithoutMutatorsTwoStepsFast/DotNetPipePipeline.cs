using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Sync;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsTwoStepsFast;

internal class DotNetPipePipeline
{
    private const int _constantToAdd = 10;
    private readonly Handler<int> _handler;
    private readonly Consumer _consumer = new();

    public DotNetPipePipeline()
    {
        _handler = CreateHandler();
    }

    private Handler<int> CreateHandler()
    {
        var pipeline = Pipelines.CreateSyncPipeline<int>("TwoStepPipeline")
            .StartWithLinear<int>("AddConstant", (input, next) =>
            {
                var result = input + _constantToAdd;
                next(result);
            })
            .HandleWith("ConsumeNumber", (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();
        return pipeline;
    }

    public void Run(int input)
    {
        _handler(input);
    }
}
