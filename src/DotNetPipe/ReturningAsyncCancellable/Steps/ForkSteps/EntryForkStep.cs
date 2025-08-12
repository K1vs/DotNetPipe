namespace K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.ForkSteps;

/// <summary>
/// Represents a fork step in a pipeline that allows branching based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TBranchAInput">The type of the input for branch A.</typeparam>
/// <typeparam name="TBranchAResult">The type of the result for branch A.</typeparam>
/// <typeparam name="TBranchBInput">The type of the input for branch B.</typeparam>
/// <typeparam name="TBranchBResult">The type of the result for branch B.</typeparam>
public sealed class EntryForkStep<TEntryStepInput, TEntryStepResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> :
    ForkStep<TEntryStepInput, TEntryStepResult, TEntryStepInput, TEntryStepResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntryForkStep{TEntryStepInput, TEntryStepResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult}"/> class.
    /// </summary>
    public EntryForkStep(string name,
        ForkSelector<TEntryStepInput, TEntryStepResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> selector,
        Func<Space, Pipeline<TBranchAInput, TBranchAResult>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput, TBranchBResult>> branchBBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchABuilder, branchBBuilder, builder)
    {
    }

    /// <inheritdoc/>
    public override Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline()
    {
        if (Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        var pipeline = new Pipeline<TEntryStepInput, TEntryStepResult>(Builder.Name, Builder.EntryStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler()
    {
        var selector = CreateStepSelector();
        var trueHandler = BranchAPipeline.Compile();
        var elseHandler = BranchBPipeline.Compile();
        Task<TEntryStepResult> Handler(TEntryStepInput input, CancellationToken ct) => selector(input, trueHandler, elseHandler, ct);
        return Handler;
    }
}



