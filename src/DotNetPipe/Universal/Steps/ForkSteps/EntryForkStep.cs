namespace K1vs.DotNetPipe.Universal.Steps.ForkSteps;

public sealed class EntryForkStep<TRootStepInput, TBranchAInput, TBranchBInput> : ForkStep<TRootStepInput, TRootStepInput, TBranchAInput, TBranchBInput>
{
    public EntryForkStep(string name,
        ForkSelector<TRootStepInput, TBranchAInput, TBranchBInput> selector,
        Func<Space, Pipeline<TBranchAInput>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput>> branchBBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchABuilder, branchBBuilder, builder)
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
        var trueHandler = BranchAPipeline.Compile();
        var elseHandler = BranchBPipeline.Compile();
        ValueTask Handler(TRootStepInput input) => selector(input, trueHandler, elseHandler);
        return Handler;
    }
}