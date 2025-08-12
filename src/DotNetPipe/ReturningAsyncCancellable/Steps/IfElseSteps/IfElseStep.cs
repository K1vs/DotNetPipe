using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.IfElseSteps;

/// <summary>
/// Represents a step in a pipeline that conditionally executes one of two pipelines based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of input for the if-else step.</typeparam>
/// <typeparam name="TResult">The type of the result for the if-else step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TIfResult">The type of result for the true branch (if condition is true).</typeparam>
/// <typeparam name="TElseInput">The type of input for the false branch (if condition is false).</typeparam>
/// <typeparam name="TElseResult">The type of result for the false branch (if condition is false).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if-else step.</typeparam>
/// <typeparam name="TNextStepResult">The type of result for the next step after the if-else step.</typeparam>
public abstract class IfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult> : ReducedPipeStep<TEntryStepInput, TEntryStepResult, TNextStepInput, TNextStepResult>
{
    private readonly IfElseSelector<TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult> _selector;
    /// <summary>
    /// Mutators for the if-else selector. These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<IfElseSelector<TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult>> Mutators { get; }
    /// <summary>
    /// Gets the pipeline that is executed when the condition is true.
    /// </summary>
    public OpenPipeline<TIfInput, TIfResult, TNextStepInput, TNextStepResult> TruePipeline { get; }
    /// <summary>
    /// Gets the pipeline that is executed when the condition is false.
    /// </summary>
    public OpenPipeline<TElseInput, TElseResult, TNextStepInput, TNextStepResult> ElsePipeline { get; }

    private protected IfElseStep(string name,
        IfElseSelector<TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepInput, TNextStepResult>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TElseResult, TNextStepInput, TNextStepResult>> elseBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        TruePipeline = trueBuilder(builder.Space);
        ElsePipeline = elseBuilder(builder.Space);
        Mutators = new StepMutators<IfElseSelector<TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult>>();
    }

    /// <summary>
    /// Creates a step selector that can be used to determine which pipeline to execute based on the input.
    /// This method applies any mutations defined in the Mutators collection to the original selector.
    /// </summary>
    /// <returns>A mutated version of the original selector.</returns>
    private protected IfElseSelector<TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}



