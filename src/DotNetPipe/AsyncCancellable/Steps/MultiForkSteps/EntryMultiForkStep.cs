namespace K1vs.DotNetPipe.AsyncCancellable.Steps.MultiForkSteps;

/// <summary>
/// Represents a multi-fork step in a pipeline that allows branching based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TBranchesInput">The type of input for the branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
public sealed class EntryMultiForkStep<TEntryStepInput, TBranchesInput, TDefaultInput> : MultiForkStep<TEntryStepInput, TEntryStepInput, TBranchesInput, TDefaultInput>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryMultiForkStep{TEntryStepInput, TBranchesInput, TDefaultInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchesBuilder">A function that builds the pipelines for the branches.</param>
    /// <param name="defaultBranchBuilder">A function that builds the pipeline for the default branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    public EntryMultiForkStep(string name,
        MultiForkSelector<TEntryStepInput, TBranchesInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput>> defaultBranchBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchesBuilder, defaultBranchBuilder, builder)
    {
    }

    /// <inheritdoc/>
    public override Pipeline<TEntryStepInput> BuildPipeline()
    {
        if (Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        return new Pipeline<TEntryStepInput>(Builder.Name, Builder.EntryStep, this, BuildHandler);
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler()
    {
        var selector = CreateStepSelector();
        var branches = Branches.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Compile()).AsReadOnly();
        var defaultBranch = DefaultBranch.Compile();
        Task Handler(TEntryStepInput input, CancellationToken ct) => selector(input, branches, defaultBranch, ct);
        return Handler;
    }
}
