using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Universal.Steps.ForkSteps;

/// <summary>
/// Represents a step in a pipeline that forks into two branches based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the fork step.</typeparam
/// <typeparam name="TBranchAInput">The type of the input for branch A.</typeparam>
/// <typeparam name="TBranchBInput">The type of the input for branch B.</typeparam>
public abstract class ForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput> : Step
{
    private readonly ForkSelector<TInput, TBranchAInput, TBranchBInput> _selector;

    /// <summary>
    /// Mutators for the fork selector.
    /// These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<ForkSelector<TInput, TBranchAInput, TBranchBInput>> Mutators { get; }

    /// <summary>
    /// Gets the pipeline for branch A.
    /// </summary>
    public Pipeline<TBranchAInput> BranchAPipeline { get; }

    /// <summary>
    /// Gets the pipeline for branch B.
    /// </summary>
    public Pipeline<TBranchBInput> BranchBPipeline { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForkStep{TEntryStepInput, TInput, TBranchAInput, TBranchBInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchABuilder">A function that builds the pipeline for branch A.</param>
    /// <param name="branchBBuilder">A function that builds the pipeline for branch B.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    private protected ForkStep(string name,
        ForkSelector<TInput, TBranchAInput, TBranchBInput> selector,
        Func<Space, Pipeline<TBranchAInput>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput>> branchBBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<ForkSelector<TInput, TBranchAInput, TBranchBInput>>();
        BranchAPipeline = branchABuilder(builder.Space);
        BranchBPipeline = branchBBuilder(builder.Space);
    }

    /// <summary>
    /// Builds the pipeline that includes this fork step.
    /// </summary>
    /// <returns>A pipeline that starts with the entry step input and ends with this fork step.</returns>
    public abstract Pipeline<TEntryStepInput> BuildPipeline();

    /// <summary>
    /// Builds the handler for this fork step.
    /// </summary>
    /// <returns>A handler that processes the input and routes it to the appropriate branch.</returns>
    internal abstract Handler<TEntryStepInput> BuildHandler();

    /// <summary>
    /// Creates a step selector that can be used to determine which branch to take based on the input.
    /// This method applies any mutations defined in the Mutators collection to the original selector.
    /// </summary>
    /// <returns>A mutated version of the original selector.</returns>
    private protected ForkSelector<TInput, TBranchAInput, TBranchBInput> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}
