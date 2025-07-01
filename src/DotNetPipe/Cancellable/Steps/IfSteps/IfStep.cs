using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Cancellable.Steps.IfSteps;

/// <summary>
/// Represents a step in a pipeline that conditionally processes input based on a selector.
/// If the condition is met, it processes the input through a specified pipeline else it continues to the next step directly.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the if step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if step.</typeparam>
public abstract class IfStep<TEntryStepInput, TInput, TIfInput, TNextStepInput> : ReducedPipeStep<TEntryStepInput, TNextStepInput>
{
    private readonly IfSelector<TInput, TIfInput, TNextStepInput> _selector;

    /// <summary>
    /// Mutators for the if selector.
    /// These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<IfSelector<TInput, TIfInput, TNextStepInput>> Mutators { get; }

    /// <summary>
    /// Gets the pipeline that is executed when the condition is true.
    /// </summary>
    public OpenPipeline<TIfInput, TNextStepInput> TruePipeline { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IfStep{TEntryStepInput, TInput, TIfInput, TNextStepInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute when the condition is true.</param>
    /// <param name="trueBuilder">A function that builds the pipeline for the true branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    private protected IfStep(string name,
        IfSelector<TInput, TIfInput, TNextStepInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<IfSelector<TInput, TIfInput, TNextStepInput>>();
        TruePipeline = trueBuilder(builder.Space);
    }

    /// <summary>
    /// Builds selectors for the if step.
    /// </summary>
    /// <returns>A selector that processes the input and determines the next step based on the condition.</returns>
    private protected IfSelector<TInput, TIfInput, TNextStepInput> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}
