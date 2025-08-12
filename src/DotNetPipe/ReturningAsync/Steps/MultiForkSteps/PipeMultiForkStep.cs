namespace K1vs.DotNetPipe.ReturningAsync.Steps.MultiForkSteps;

/// <summary>
/// Represents a step inside a pipeline that allows for multiple branches based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the multi-fork step.</typeparam>
/// <typeparam name="TResult">The type of the result for the multi-fork step.</typeparam>
/// <typeparam name="TBranchesInput">The type of input for the branches.</typeparam>
/// <typeparam name="TBranchesResult">The type of result for the branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TDefaultResult">The type of result for the default branch.</typeparam>
public sealed class PipeMultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> : MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>
{
    /// <summary>
    /// Gets the previous step in the pipeline that leads to this multi-fork step.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> PreviousStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeMultiForkStep{TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult}"/> class.
    /// </summary>
    /// <param name="previous">The previous step in the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchesBuilder">A function that builds the pipelines for the branches.</param>
    /// <param name="defaultBranchBuilder">A function that builds the pipeline for the default branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal PipeMultiForkStep(ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> previous,
        string name,
        MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBranchBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchesBuilder, defaultBranchBuilder, builder)
    {
        PreviousStep = previous;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler()
    {
        var selector = CreateStepSelector();
        var branches = Branches.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Compile());
        var defaultBranch = DefaultBranch.Compile();
        Task<TResult> Handler(TInput input) => selector(input, branches, defaultBranch);
        return PreviousStep.BuildHandler(Handler);
    }
}

