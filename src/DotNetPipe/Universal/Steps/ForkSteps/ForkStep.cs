using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Universal.Steps.ForkSteps;

public abstract class ForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput> : Step
{
    private readonly ForkSelector<TInput, TBranchAInput, TBranchBInput> _selector;

    public StepMutators<ForkSelector<TInput, TBranchAInput, TBranchBInput>> Mutators { get; }

    public Pipeline<TBranchAInput> BranchAPipeline { get; }

    public Pipeline<TBranchBInput> BranchBPipeline { get; }

    private protected ForkStep(string name,
        ForkSelector<TInput, TBranchAInput, TBranchBInput> selector,
        Func<Space, Pipeline<TBranchAInput>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput>> branchBBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<ForkSelector<TInput, TBranchAInput, TBranchBInput>>();
        BranchAPipeline = branchABuilder(builder.Space);
        BranchBPipeline = branchBBuilder(builder.Space);
    }

    public abstract Pipeline<TRootStepInput> BuildPipeline();

    internal abstract Handler<TRootStepInput> BuildHandler();

    private protected ForkSelector<TInput, TBranchAInput, TBranchBInput> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}
