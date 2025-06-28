namespace K1vs.DotNetPipe.Async.Steps.ForkSteps;

/// <summary>
/// Represents a fork step pipeline a pipeline that allows branching into two separate pipelines based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the fork step.</typeparam>
/// <typeparam name="TBranchAInput">The type of the input for branch A.</typeparam>
/// <typeparam name="TBranchBInput">The type of the input for branch B.</typeparam>
public sealed class PipeForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput> : ForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput>
{
    /// <summary>
    /// Gets the previous step in the pipeline, which is the step that feeds input into this fork step.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TInput> PreviousStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeForkStep{TEntryStepInput, TInput, TBranchAInput, TBranchBInput}"/> class.
    /// </summary>
    /// <param name="previous">The previous step in the pipeline that provides input to this fork step.</param>
    /// <param name="name">The name of the fork step.</param>
    /// <param name="selector">The selector that determines which branch to take based on the input.</param>
    /// <param name="branchABuilder">A function that builds the pipeline for branch A.</param>
    /// <param name="branchBBuilder">A function that builds the pipeline for branch B.</param>
    /// <param name="builder">The pipeline builder that contains the space and other configurations.</param>
    internal PipeForkStep(ReducedPipeStep<TEntryStepInput, TInput> previous,
        string name,
        ForkSelector<TInput, TBranchAInput, TBranchBInput> selector,
        Func<Space, Pipeline<TBranchAInput>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput>> branchBBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchABuilder, branchBBuilder, builder)
    {
        PreviousStep = previous;
    }

    /// <inheritdoc/>
    public override Pipeline<TEntryStepInput> BuildPipeline()
    {
        if (PreviousStep is null)
        {
            throw new InvalidOperationException("Previous step is not set");
        }
        var pipeline = new Pipeline<TEntryStepInput>(Builder.Name, PreviousStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler()
    {
        var selector = CreateStepSelector();
        var trueHandler = BranchAPipeline.Compile();
        var elseHandler = BranchBPipeline.Compile();
        Task Handler(TInput input) => selector(input, trueHandler, elseHandler);
        return PreviousStep.BuildHandler(Handler);
    }
}
