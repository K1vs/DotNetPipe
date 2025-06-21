using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Universal.Steps.IfElseSteps;

public abstract class IfElseStep<TRootStepInput, TInput, TIfInput, TElseInput, TNextStepInput> : ReducedPipeStep<TRootStepInput, TNextStepInput>
{
    private readonly IfElseSelector<TInput, TIfInput, TElseInput> _selector;

    public StepMutators<IfElseSelector<TInput, TIfInput, TElseInput>> Mutators { get; }

    public OpenPipeline<TIfInput, TNextStepInput> TruePipeline { get; }

    public OpenPipeline<TElseInput, TNextStepInput> ElsePipeline { get; }

    public IfElseStep(string name,
        IfElseSelector<TInput, TIfInput, TElseInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TNextStepInput>> elseBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        TruePipeline = trueBuilder(builder.Space);
        ElsePipeline = elseBuilder(builder.Space);
        Mutators = new StepMutators<IfElseSelector<TInput, TIfInput, TElseInput>>();
    }

    private protected IfElseSelector<TInput, TIfInput, TElseInput> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}
