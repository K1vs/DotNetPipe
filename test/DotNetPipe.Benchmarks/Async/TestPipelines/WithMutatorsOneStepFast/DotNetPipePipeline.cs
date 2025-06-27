using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Async;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsOneStepFast;

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
        var pipeline = Pipelines.CreateAsyncPipeline<int>("TestPipeline")
            .StartWithHandler("TestHandler", async (input) => _consumer.Consume(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureTestPipelineMutators);
            });
        return pipeline;
    }

    private void ConfigureTestPipelineMutators(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int>("TestPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1; // Add 1 to input before processing
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }

    public async Task Run(int input)
    {
        await _handler(input);
    }
}
