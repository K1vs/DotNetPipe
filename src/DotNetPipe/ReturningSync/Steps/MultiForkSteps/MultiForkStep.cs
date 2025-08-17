using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.ReturningSync.Steps.MultiForkSteps;

/// <summary>
/// Represents a step in a pipeline that can fork into multiple branches based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the multi-fork step.</typeparam>
/// <typeparam name="TResult">The type of the result for the multi-fork step.</typeparam>
/// <typeparam name="TBranchesInput">The type of the input for the branches.</typeparam>
/// <typeparam name="TBranchesResult">The type of the result for the branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of the input for the default branch.</typeparam>
/// <typeparam name="TDefaultResult">The type of the result for the default branch.</typeparam>
public abstract class MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> : Step
{
    private readonly MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> _selector;

    /// <summary>
    /// Mutators for the multi-fork selector.
    /// These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>> Mutators { get; }

    /// <summary>
    /// Gets the branches of the multi-fork step.
    /// </summary>
    public IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>> Branches { get; }

    /// <summary>
    /// Gets the default branch of the multi-fork step.
    /// </summary>
    public Pipeline<TDefaultInput, TDefaultResult> DefaultBranch { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiForkStep{TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchesBuilder">A function that builds the pipelines for the branches.</param>
    /// <param name="defaultBranchBuilder">A function that builds the pipeline for the default branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    private protected MultiForkStep(string name,
        MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBranchBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>>();
        Branches = branchesBuilder(builder.Space);
        DefaultBranch = defaultBranchBuilder(builder.Space);
    }

    /// <summary>
    /// Builds the pipeline that includes this multi-fork step.
    /// </summary>
    /// <returns>A pipeline that starts with the entry step input and ends with this multi-fork step.</returns>
    public abstract Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline();

    /// <summary>
    /// Builds the handler based on this multi-fork step.
    /// </summary>
    /// <returns>A handler that processes the input and performs the defined operations.</returns>
    internal abstract Handler<TEntryStepInput, TEntryStepResult> BuildHandler();

    /// <summary>
    /// Creates selector for the multi-fork step including any mutations defined in the Mutators collection.
    /// </summary>
    /// <returns>A mutated version of the original selector.</returns>
    private protected MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> CreateStepSelector() => Mutators.MutateDelegate(_selector);
}

