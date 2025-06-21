using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Universal.Steps.MultiForkSteps;

public abstract class MultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput> : Step
{
    private readonly MultiForkSelector<TInput, TBranchesInput, TDefaultInput> _selector;

    public StepMutators<MultiForkSelector<TInput, TBranchesInput, TDefaultInput>> Mutators { get; }

    public IReadOnlyDictionary<string, Pipeline<TBranchesInput>> Branches { get; }

    public Pipeline<TDefaultInput> DefaultBranch { get; }

    private protected MultiForkStep(string name,
        MultiForkSelector<TInput, TBranchesInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput>> defaultBranchBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<MultiForkSelector<TInput, TBranchesInput, TDefaultInput>>();
        Branches = branchesBuilder(builder.Space);
        DefaultBranch = defaultBranchBuilder(builder.Space);
    }

    public abstract Pipeline<TRootStepInput> BuildPipeline();

    internal abstract Handler<TRootStepInput> BuildHandler();

    private protected MultiForkSelector<TInput, TBranchesInput, TDefaultInput> CreateStepSelector() => Mutators.MutateDelegate(_selector);
}