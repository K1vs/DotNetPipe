namespace K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.ForkSteps;

/// <summary>
/// Represents a fork step that follows a previous reduced pipe step.
/// Splits the pipeline into two branches and compiles both branches as sub-pipelines.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input of the pipeline entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result of the pipeline entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for this step.</typeparam>
/// <typeparam name="TResult">The type of the result produced by this step.</typeparam>
/// <typeparam name="TBranchAInput">The type of the input for branch A.</typeparam>
/// <typeparam name="TBranchAResult">The type of the result for branch A.</typeparam>
/// <typeparam name="TBranchBInput">The type of the input for branch B.</typeparam>
/// <typeparam name="TBranchBResult">The type of the result for branch B.</typeparam>
public sealed class PipeForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> : ForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>
{
    /// <summary>
    /// Gets the previous reduced pipe step in the pipeline.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> PreviousStep { get; }

    internal PipeForkStep(ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> previous,
        string name,
        ForkSelector<TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> selector,
        Func<Space, Pipeline<TBranchAInput, TBranchAResult>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput, TBranchBResult>> branchBBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchABuilder, branchBBuilder, builder)
    {
        PreviousStep = previous;
    }

    /// <summary>
    /// Builds a closed pipeline using the current fork step and the previous step.
    /// </summary>
    /// <returns>The closed pipeline instance.</returns>
    public override Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline()
    {
        if (PreviousStep is null)
        {
            throw new InvalidOperationException("Previous step is not set");
        }
        var pipeline = new Pipeline<TEntryStepInput, TEntryStepResult>(Builder.Name, PreviousStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler()
    {
        var selector = CreateStepSelector();
        var trueHandler = BranchAPipeline.Compile();
        var elseHandler = BranchBPipeline.Compile();
        Task<TResult> Handler(TInput input, CancellationToken ct) => selector(input, trueHandler, elseHandler, ct);
        return PreviousStep.BuildHandler(Handler);
    }
}



