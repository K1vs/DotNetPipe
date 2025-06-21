namespace K1vs.DotNetPipe.Universal.Steps.MultiForkSteps;

public sealed class EntryMultiForkStep<TRootStepInput, TBranchesInput, TDefaultInput> : MultiForkStep<TRootStepInput, TRootStepInput, TBranchesInput, TDefaultInput>
{
    public EntryMultiForkStep(string name,
        MultiForkSelector<TRootStepInput, TBranchesInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput>> defaultBranchBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchesBuilder, defaultBranchBuilder, builder)
    {
    }

    public override Pipeline<TRootStepInput> BuildPipeline()
    {
        if(Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        return new Pipeline<TRootStepInput>(Builder.Name, Builder.EntryStep, this, BuildHandler);
    }

    internal override Handler<TRootStepInput> BuildHandler()
    {
        var selector = CreateStepSelector();
        var branches = Branches.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Compile()).AsReadOnly();
        var defaultBranch = DefaultBranch.Compile();
        ValueTask Handler(TRootStepInput input) => selector(input, branches, defaultBranch);
        return Handler;
    }
}