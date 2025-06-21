namespace K1vs.DotNetPipe.Universal.Steps.MultiForkSteps;

public sealed class PipeMultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput> : MultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput>
{
    public ReducedPipeStep<TRootStepInput, TInput> PreviousStep { get; }

    internal PipeMultiForkStep(ReducedPipeStep<TRootStepInput, TInput> previous,
        string name,
        MultiForkSelector<TInput, TBranchesInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput>> defaultBranchBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchesBuilder, defaultBranchBuilder, builder)
    {
        PreviousStep = previous;
    }

    public override Pipeline<TRootStepInput> BuildPipeline()
    {
        if(PreviousStep is null)
        {
            throw new InvalidOperationException("Previous step is not set");
        }
        return new Pipeline<TRootStepInput>(Builder.Name, PreviousStep, this, BuildHandler);
    }

    internal override Handler<TRootStepInput> BuildHandler()
    {
        var selector = CreateStepSelector();
        var branches = Branches.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Compile()).AsReadOnly();
        var defaultBranch = DefaultBranch.Compile();
        ValueTask Handler(TInput input) => selector(input, branches, defaultBranch);
        return PreviousStep.BuildHandler(Handler);
    }
}