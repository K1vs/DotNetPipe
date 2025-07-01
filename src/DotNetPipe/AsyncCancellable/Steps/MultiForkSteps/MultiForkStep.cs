using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.AsyncCancellable.Steps.MultiForkSteps;

/// <summary>
/// Represents a step in a pipeline that can fork into multiple branches based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the multi-fork step.</typeparam>
/// <typeparam name="TBranchesInput">The type of the input for the branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of the input for the default branch.</typeparam>
public abstract class MultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput> : Step
{
    private readonly MultiForkSelector<TInput, TBranchesInput, TDefaultInput> _selector;

    /// <summary>
    /// Mutators for the multi-fork selector.
    /// These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<MultiForkSelector<TInput, TBranchesInput, TDefaultInput>> Mutators { get; }

    /// <summary>
    /// Gets the branches of the multi-fork step.
    /// </summary>
    public IReadOnlyDictionary<string, Pipeline<TBranchesInput>> Branches { get; }

    /// <summary>
    /// Gets the default branch of the multi-fork step.
    /// </summary>
    public Pipeline<TDefaultInput> DefaultBranch { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiForkStep{TEntryStepInput, TInput, TBranchesInput, TDefaultInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchesBuilder">A function that builds the pipelines for the branches.</param>
    /// <param name="defaultBranchBuilder">A function that builds the pipeline for the default branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    private protected MultiForkStep(string name,
        MultiForkSelector<TInput, TBranchesInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput>> defaultBranchBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<MultiForkSelector<TInput, TBranchesInput, TDefaultInput>>();
        Branches = branchesBuilder(builder.Space);
        DefaultBranch = defaultBranchBuilder(builder.Space);
    }

    /// <summary>
    /// Builds the pipeline that includes this multi-fork step.
    /// </summary>
    /// <returns>A pipeline that starts with the entry step input and ends with this multi-fork step.</returns>
    public abstract Pipeline<TEntryStepInput> BuildPipeline();

    /// <summary>
    /// Builds the handler based on this multi-fork step.
    /// </summary>
    /// <returns>A handler that processes the input and performs the defined operations.</returns>
    internal abstract Handler<TEntryStepInput> BuildHandler();

    /// <summary>
    /// Creates selector for the multi-fork step including any mutations defined in the Mutators collection.
    /// </summary>
    /// <returns>A mutated version of the original selector.</returns>
    private protected MultiForkSelector<TInput, TBranchesInput, TDefaultInput> CreateStepSelector() => Mutators.MutateDelegate(_selector);
}
