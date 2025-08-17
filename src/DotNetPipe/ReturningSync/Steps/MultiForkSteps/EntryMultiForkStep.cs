namespace K1vs.DotNetPipe.ReturningSync.Steps.MultiForkSteps;

/// <summary>
/// Represents a multi-fork step in a pipeline that allows branching based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TBranchesInput">The type of input for the branches.</typeparam>
/// <typeparam name="TBranchesResult">The type of result for the branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TDefaultResult">The type of result for the default branch.</typeparam>
public sealed class EntryMultiForkStep<TEntryStepInput, TEntryStepResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> : MultiForkStep<TEntryStepInput, TEntryStepResult, TEntryStepInput, TEntryStepResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryMultiForkStep{TEntryStepInput, TEntryStepResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchesBuilder">A function that builds the pipelines for the branches.</param>
    /// <param name="defaultBranchBuilder">A function that builds the pipeline for the default branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    public EntryMultiForkStep(string name,
        MultiForkSelector<TEntryStepInput, TEntryStepResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBranchBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchesBuilder, defaultBranchBuilder, builder)
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
        var branches = Branches.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Compile());
        var defaultBranch = DefaultBranch.Compile();
        TEntryStepResult Handler(TEntryStepInput input) => selector(input, branches, defaultBranch);
        return Handler;
    }
}

