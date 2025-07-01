namespace K1vs.DotNetPipe.AsyncCancellable.Steps.IfSteps;

/// <summary>
/// Represents a step inside a pipeline that conditionally processes input based on a selector.
/// If the condition is met, it processes the input through a specified pipeline else it continues to the next step directly.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the if step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if step.</typeparam>
public sealed class PipeIfStep<TEntryStepInput, TInput, TIfInput, TNextStepInput> : IfStep<TEntryStepInput, TInput, TIfInput, TNextStepInput>
{
    /// <summary>
    /// Gets the previous step in the pipeline before this if step.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TInput> PreviousStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeIfStep{TEntryStepInput, TInput, TIfInput, TNextStepInput}"/> class.
    /// </summary>
    /// <param name="previousStep">The previous step in the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute when the condition is true.</param>
    /// <param name="trueBuilder">A function that builds the pipeline for the true branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal PipeIfStep(ReducedPipeStep<TEntryStepInput, TInput> previousStep,
        string name,
        IfSelector<TInput, TIfInput, TNextStepInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, builder)
    {
        PreviousStep = previousStep;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var trueHandler = TruePipeline.BuildHandler(handler);
        var selector = CreateStepSelector();
        Task Handler(TInput input, CancellationToken ct) => selector(input, trueHandler, handler, ct);
        return PreviousStep.BuildHandler(Handler);
    }
}
