using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Universal.Steps.HandlerSteps;

public abstract class HandlerStep<TRootStepInput, TInput>: Step
{
    private readonly Handler<TInput> _originalHandler;

    public StepMutators<Handler<TInput>> Mutators { get; }

    private protected HandlerStep(string name, Handler<TInput> handler, PipelineBuilder builder)
        : base(name, builder)
    {
        _originalHandler = handler;
        Mutators = new StepMutators<Handler<TInput>>();
    }

    public abstract Pipeline<TRootStepInput> BuildPipeline();

    internal abstract Handler<TRootStepInput> BuildHandler();

    private protected Handler<TInput> CreateStepHandler()
    {
        var handler = Mutators.MutateDelegate(_originalHandler);
        return handler;
    }
}
