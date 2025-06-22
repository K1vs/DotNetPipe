using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Universal;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsTwoStepsFast;

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
        var pipeline = Pipelines.CreatePipeline<int>("TwoStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + _constantToAdd;
                await next(result);
            })
            .HandleWith("ConsumeNumber", async (input) => _consumer.Consume(input))
            .BuildPipeline().Compile();
        return pipeline;
    }

    public async ValueTask Run(int input)
    {
        await _handler(input);
    }
}
