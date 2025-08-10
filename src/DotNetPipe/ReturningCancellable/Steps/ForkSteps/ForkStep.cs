using K1vs.DotNetPipe.Mutations;
namespace K1vs.DotNetPipe.ReturningCancellable.Steps.ForkSteps;

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
    /// Mutators for the fork selector. These allow for modifying the behavior of the selector dynamically.
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
    public abstract Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline();
    /// <summary>
    /// Builds the handler for this fork step.
    /// </summary>
    internal abstract Handler<TEntryStepInput, TEntryStepResult> BuildHandler();

    /// <summary>
    /// Creates a step selector that can be used to determine which branch to take based on the input.
    /// </summary>
    private protected ForkSelector<TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}


