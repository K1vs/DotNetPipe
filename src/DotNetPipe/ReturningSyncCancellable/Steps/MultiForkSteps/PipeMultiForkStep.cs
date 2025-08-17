namespace K1vs.DotNetPipe.ReturningSyncCancellable.Steps.MultiForkSteps;

/// <summary>
/// Represents a multi-fork step that follows a previous reduced pipe step.
/// Splits the pipeline into multiple named branches with a default branch.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input of the pipeline entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result of the pipeline entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for this step.</typeparam>
/// <typeparam name="TResult">The type of the result produced by this step.</typeparam>
/// <typeparam name="TBranchesInput">The type of the input of the branch pipelines.</typeparam>
/// <typeparam name="TBranchesResult">The type of the result of the branch pipelines.</typeparam>
/// <typeparam name="TDefaultInput">The type of the input of the default pipeline.</typeparam>
/// <typeparam name="TDefaultResult">The type of the result of the default pipeline.</typeparam>
public sealed class PipeMultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> : MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>
{
    /// <summary>
    /// Gets the previous reduced pipe step in the pipeline.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> PreviousStep { get; }

    internal PipeMultiForkStep(ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> previous,
        string name,
        MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchesBuilder, defaultBuilder, builder)
    {
        PreviousStep = previous;
    }

    /// <summary>
    /// Builds a closed pipeline using the current multi-fork step and the previous step.
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

    /// <summary>
    /// Builds the handler for this step by compiling all branches and the default pipeline.
    /// </summary>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler()
    {
        var selector = CreateStepSelector();
        var branchesHandlers = BranchesPipelines.ToDictionary(kv => kv.Key, kv => kv.Value.Compile());
        var defaultHandler = DefaultPipeline.Compile();
        TResult Handler(TInput input, CancellationToken ct) => selector(input, branchesHandlers, defaultHandler, ct);
        return PreviousStep.BuildHandler(Handler);
    }
}



