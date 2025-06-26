using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Universal;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsTwoStepsFast;

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
        var pipeline = Pipelines.CreatePipeline<int>("TestTwoStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + _constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => _consumer.Consume(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureTwoStepPipelineMutators);
            });
        return pipeline;
    }

    private void ConfigureTwoStepPipelineMutators(Space space)
    {
        // Mutator for linear step - multiply input by 2 before processing
        var linearStep = space.GetRequiredLinearStep<int, int, int>("TestTwoStepPipeline", "AddConstant");
        var linearMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply mutation to input first - multiply by 2
                input *= 2;
                await pipe(input, next);
            };
        });
        linearStep.Mutators.AddMutator(linearMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1
        var handlerStep = space.GetRequiredHandlerStep<int, int>("TestTwoStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    public async ValueTask Run(int input)
    {
        await _handler(input);
    }
}
