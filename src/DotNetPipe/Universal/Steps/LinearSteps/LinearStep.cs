using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Universal.Steps.LinearSteps;

public abstract class LinearStep<TRootStepInput, TInput, TNextInput>: ReducedPipeStep<TRootStepInput, TNextInput>
{
    private readonly Pipe<TInput, TNextInput> _originalPipe;

    public StepMutators<Pipe<TInput, TNextInput>> Mutators { get; }

    private protected LinearStep(string name, Pipe<TInput, TNextInput> originalPipe, PipelineBuilder builder)
        : base(name, builder)
    {
        _originalPipe = originalPipe;
        Mutators = new StepMutators<Pipe<TInput, TNextInput>>();
    }

    private protected Pipe<TInput, TNextInput> CreateStepPipe() => Mutators.MutateDelegate(_originalPipe);
}
