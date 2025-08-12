using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.MultiForkSteps;

/// <summary>
/// Represents a step that splits the processing into multiple branches based on a selector, with a default branch.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the multi-fork step.</typeparam>
/// <typeparam name="TResult">The type of the result for the multi-fork step.</typeparam>
/// <typeparam name="TBranchesInput">The type of input for branch handlers.</typeparam>
/// <typeparam name="TBranchesResult">The type of result for branch handlers.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TDefaultResult">The type of result for the default branch.</typeparam>
public abstract class MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> : Step
{
    private readonly MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> _selector;
    /// <summary>
    /// Mutators for the multi-fork selector. These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>> Mutators { get; }
    /// <summary>
    /// Gets the pipelines for the branches.
    /// </summary>
    public IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>> BranchesPipelines { get; }
    /// <summary>
    /// Gets the default pipeline for the multi-fork step.
    /// </summary>
    public Pipeline<TDefaultInput, TDefaultResult> DefaultPipeline { get; }

    private protected MultiForkStep(string name,
        MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>>();
        BranchesPipelines = branchesBuilder(builder.Space);
        DefaultPipeline = defaultBuilder(builder.Space);
    }

    /// <summary>
    /// Builds the pipeline that includes this multi-fork step.
    /// </summary>
    public abstract Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline();
    /// <summary>
    /// Builds the handler for this multi-fork step.
    /// </summary>
    internal abstract Handler<TEntryStepInput, TEntryStepResult> BuildHandler();

    /// <summary>
    /// Creates a step selector that can be used to determine which branch to take based on the input.
    /// </summary>
    private protected MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}



