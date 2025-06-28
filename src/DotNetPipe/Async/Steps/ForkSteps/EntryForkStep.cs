namespace K1vs.DotNetPipe.Async.Steps.ForkSteps;

/// <summary>
/// Represents a fork step in a pipeline that allows branching based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TBranchAInput">The type of the input for branch A.</typeparam>
/// <typeparam name="TBranchBInput">The type of the input for branch B.</typeparam>
public sealed class EntryForkStep<TEntryStepInput, TBranchAInput, TBranchBInput> : ForkStep<TEntryStepInput, TEntryStepInput, TBranchAInput, TBranchBInput>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntryForkStep{TEntryStepInput, TBranchAInput, TBranchBInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the fork step.</param>
    /// <param name="selector">The selector that determines which branch to take based on the input.</param>
    /// <param name="branchABuilder">A function that builds the pipeline for branch A.</param>
    /// <param name="branchBBuilder">A function that builds the pipeline for branch B.</param>
    /// <param name="builder">The pipeline builder that contains the space and other configurations.</param>
    public EntryForkStep(string name,
        ForkSelector<TEntryStepInput, TBranchAInput, TBranchBInput> selector,
        Func<Space, Pipeline<TBranchAInput>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput>> branchBBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchABuilder, branchBBuilder, builder)
    {
    }

    /// <inheritdoc/>
    public override Pipeline<TEntryStepInput> BuildPipeline()
    {
        if (Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        var pipeline = new Pipeline<TEntryStepInput>(Builder.Name, Builder.EntryStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler()
    {
        var selector = CreateStepSelector();
        var trueHandler = BranchAPipeline.Compile();
        var elseHandler = BranchBPipeline.Compile();
        Task Handler(TEntryStepInput input) => selector(input, trueHandler, elseHandler);
        return Handler;
    }
}
