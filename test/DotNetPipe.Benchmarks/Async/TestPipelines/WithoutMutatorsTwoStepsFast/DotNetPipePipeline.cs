using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Async;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsTwoStepsFast;

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
        var pipeline = Pipelines.CreateAsyncPipeline<int>("TwoStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + _constantToAdd;
                await next(result);
            })
            .HandleWith("ConsumeNumber", async (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();
        return pipeline;
    }

    public async Task Run(int input)
    {
        await _handler(input);
    }
}
