using BenchmarkDotNet.Engines;
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Async;
using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsIfElseTwoStepsFast;

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
        var pipeline = Pipelines.CreateAsyncPipeline<int>("IfTwoStepPipeline")
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigurePipelineMutators);
            });
        return pipeline;
    }

    private void ConfigurePipelineMutators(Space space)
    {
        // Mutator for CheckNegative If step - modify the condition
        var ifStep = space.GetRequiredIfStep<int, int, int, int>("IfTwoStepPipeline", "CheckNegative");
        var ifMutator = new StepMutator<IfSelector<int, int, int>>("CheckNegativeMutator", 1, (selector) =>
        {
            return async (input, conditionalNext, next) =>
            {
                // Modified logic: numbers <= 0 (instead of < 0) go to conditional branch
                if (input <= 0)
                {
                    await conditionalNext(input);
                }
                else
                {
                    await next(input);
                }
            };
        });
        ifStep.Mutators.AddMutator(ifMutator, AddingMode.ExactPlace);

        // Mutator for MultiplyByConstant step - add 1 before multiplying
        var multiplyStep = space.GetRequiredLinearStep<int, int, int>("MultiplyNegative", "MultiplyByConstant");
        var multiplyMutator = new StepMutator<Pipe<int, int>>("MultiplyByConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Add 1 to input before applying original logic
                input += 1;
                await pipe(input, next);
            };
        });
        multiplyStep.Mutators.AddMutator(multiplyMutator, AddingMode.ExactPlace);

        // Mutator for ConsumeNumber handler - pass through as-is
        var handlerStep = space.GetRequiredHandlerStep<int, int>("IfTwoStepPipeline", "ConsumeNumber");
        var handlerMutator = new StepMutator<Handler<int>>("ConsumeNumberMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    public async Task Run(int input)
    {
        await _handler(input);
    }
}
