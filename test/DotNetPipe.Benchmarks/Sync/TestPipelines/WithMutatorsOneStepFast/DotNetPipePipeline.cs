using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Sync;

namespace DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsOneStepFast;

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
        var pipeline = Pipelines.CreateSyncPipeline<int>("TestPipeline")
            .StartWithHandler("TestHandler", (input) => _consumer.Consume(input))
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
            return (input) =>
            {
                input += 1; // Add 1 to input before processing
                handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }

    public void Run(int input)
    {
        _handler(input);
    }
}
