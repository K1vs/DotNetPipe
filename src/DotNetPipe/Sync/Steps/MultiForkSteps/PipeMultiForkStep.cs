namespace K1vs.DotNetPipe.Sync.Steps.MultiForkSteps;

/// <summary>
/// Represents a step inside a pipeline that allows for multiple branches based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the multi-fork step.</typeparam>
/// <typeparam name="TBranchesInput">The type of input for the branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
public sealed class PipeMultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput> : MultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput>
{
    /// <summary>
    /// Gets the previous step in the pipeline that leads to this multi-fork step.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TInput> PreviousStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeMultiForkStep{TEntryStepInput, TInput, TBranchesInput, TDefaultInput}"/> class.
    /// </summary>
    /// <param name="previous">The previous step in the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchesBuilder">A function that builds the pipelines for the branches.</param>
    /// <param name="defaultBranchBuilder">A function that builds the pipeline for the default branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal PipeMultiForkStep(ReducedPipeStep<TEntryStepInput, TInput> previous,
        string name,
        MultiForkSelector<TInput, TBranchesInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput>> defaultBranchBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchesBuilder, defaultBranchBuilder, builder)
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
        return new Pipeline<TEntryStepInput>(Builder.Name, PreviousStep, this, BuildHandler);
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler()
    {
        var selector = CreateStepSelector();
        var branches = Branches.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Compile()).AsReadOnly();
        var defaultBranch = DefaultBranch.Compile();
        void Handler(TInput input) => selector(input, branches, defaultBranch);
        return PreviousStep.BuildHandler(Handler);
    }
}
