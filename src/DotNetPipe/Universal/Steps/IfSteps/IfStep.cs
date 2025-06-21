using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Universal.Steps.IfSteps;

public abstract class IfStep<TRootStepInput, TInput, TIfInput, TNextStepInput> : ReducedPipeStep<TRootStepInput, TNextStepInput>
{
    private readonly IfSelector<TInput, TIfInput, TNextStepInput> _selector;

    public StepMutators<IfSelector<TInput, TIfInput, TNextStepInput>> Mutators { get; }

    public OpenPipeline<TIfInput, TNextStepInput> TruePipeline { get; }

    private protected IfStep(string name,
        IfSelector<TInput, TIfInput, TNextStepInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<IfSelector<TInput, TIfInput, TNextStepInput>>();
        TruePipeline = trueBuilder(builder.Space);
    }

    private protected IfSelector<TInput, TIfInput, TNextStepInput> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}
