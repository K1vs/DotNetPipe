namespace K1vs.DotNetPipe.Universal.Steps.ForkSteps;

public sealed class PipeForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput> : ForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput>
{
    public ReducedPipeStep<TRootStepInput, TInput> PreviousStep { get; }

    internal PipeForkStep(ReducedPipeStep<TRootStepInput, TInput> previous,
        string name,
        ForkSelector<TInput, TBranchAInput, TBranchBInput> selector,
        Func<Space, Pipeline<TBranchAInput>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput>> branchBBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchABuilder, branchBBuilder, builder)
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
        var trueHandler = BranchAPipeline.Compile();
        var elseHandler = BranchBPipeline.Compile();
        ValueTask Handler(TInput input) => selector(input, trueHandler, elseHandler);
        return PreviousStep.BuildHandler(Handler);
    }
}