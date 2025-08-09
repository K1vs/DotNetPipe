using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Returning;

namespace K1vs.DotNetPipe.Returning.Steps.ForkSteps;

/// <summary>
/// Represents a step in a pipeline that forks into two branches based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the fork step.</typeparam>
/// <typeparam name="TResult">The type of the result for the fork step.</typeparam>
/// <typeparam name="TBranchAInput">The type of the input for branch A.</typeparam>
/// <typeparam name="TBranchAResult">The type of the result for branch A.</typeparam>
/// <typeparam name="TBranchBInput">The type of the input for branch B.</typeparam>
/// <typeparam name="TBranchBResult">The type of the result for branch B.</typeparam>
public abstract class ForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> : Step
{
    private readonly ForkSelector<TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> _selector;

    /// <summary>
    /// Mutators for the fork selector.
    /// These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<ForkSelector<TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>> Mutators { get; }

    /// <summary>
    /// Gets the pipeline for branch A.
    /// </summary>
    public Pipeline<TBranchAInput, TBranchAResult> BranchAPipeline { get; }

    /// <summary>
    /// Gets the pipeline for branch B.
    /// </summary>
    public Pipeline<TBranchBInput, TBranchBResult> BranchBPipeline { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForkStep{TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchABuilder">A function that builds the pipeline for branch A.</param>
    /// <param name="branchBBuilder">A function that builds the pipeline for branch B.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    private protected ForkStep(string name,
        ForkSelector<TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> selector,
        Func<Space, Pipeline<TBranchAInput, TBranchAResult>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput, TBranchBResult>> branchBBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<ForkSelector<TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>>();
        BranchAPipeline = branchABuilder(builder.Space);
        BranchBPipeline = branchBBuilder(builder.Space);
    }

    /// <summary>
    /// Builds the pipeline that includes this fork step.
    /// </summary>
    /// <returns>A pipeline that starts with the entry step input and ends with this fork step.</returns>
    public abstract Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline();

    /// <summary>
    /// Builds the handler for this fork step.
    /// </summary>
    /// <returns>A handler that processes the input and routes it to the appropriate branch.</returns>
    internal abstract Handler<TEntryStepInput, TEntryStepResult> BuildHandler();

    /// <summary>
    /// Creates a step selector that can be used to determine which branch to take based on the input.
    /// This method applies any mutations defined in the Mutators collection to the original selector.
    /// </summary>
    /// <returns>A mutated version of the original selector.</returns>
    private protected ForkSelector<TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}
