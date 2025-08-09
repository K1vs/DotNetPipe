using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Returning.Steps.IfSteps;

/// <summary>
/// Represents a step in a pipeline that conditionally processes input based on a selector.
/// If the condition is met, it processes the input through a specified pipeline else it continues to the next step directly.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the if step.</typeparam>
/// <typeparam name="TResult">The type of the result for the if step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TIfResult">The type of result for the true branch (if condition is true).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if step.</typeparam>
/// <typeparam name="TNextStepResult">The type of result for the next step after the if step.</typeparam>
public abstract class IfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult> : ReducedPipeStep<TEntryStepInput, TEntryStepResult, TNextStepInput, TNextStepResult>
{
    private readonly IfSelector<TInput, TIfInput, TNextStepInput, TResult, TIfResult, TNextStepResult> _selector;

    /// <summary>
    /// Mutators for the if selector.
    /// These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<IfSelector<TInput, TIfInput, TNextStepInput, TResult, TIfResult, TNextStepResult>> Mutators { get; }

    /// <summary>
    /// Gets the pipeline that is executed when the condition is true.
    /// </summary>
    public OpenPipeline<TIfInput, TIfResult, TNextStepInput, TNextStepResult> TruePipeline { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IfStep{TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute when the condition is true.</param>
    /// <param name="trueBuilder">A function that builds the pipeline for the true branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    private protected IfStep(string name,
        IfSelector<TInput, TIfInput, TNextStepInput, TResult, TIfResult, TNextStepResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepInput, TNextStepResult>> trueBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<IfSelector<TInput, TIfInput, TNextStepInput, TResult, TIfResult, TNextStepResult>>();
        TruePipeline = trueBuilder(builder.Space);
    }

    /// <summary>
    /// Builds selectors for the if step.
    /// </summary>
    /// <returns>A selector that processes the input and determines the next step based on the condition.</returns>
    private protected IfSelector<TInput, TIfInput, TNextStepInput, TResult, TIfResult, TNextStepResult> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}
