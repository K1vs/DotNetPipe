using System.Threading;

namespace K1vs.DotNetPipe.ReturningCancellable.Steps.IfSteps;

/// <summary>
/// Represents a step inside a pipeline that conditionally processes input based on a selector.
/// If the condition is met, it processes the input through a specified pipeline else it continues to the next step directly.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the if step.</typeparam>
/// <typeparam name="TResult">The type of the result for the if step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TIfResult">The type of result for the true branch (if condition is true).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if step.</typeparam>
/// <typeparam name="TNextStepResult">The type of the result for the next step after the if step.</typeparam>
public sealed class PipeIfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult> : IfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult>
{
    /// <summary>
    /// Gets the previous step in the pipeline before this if step.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> PreviousStep { get; }

    internal PipeIfStep(ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> previousStep,
        string name,
        IfSelector<TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepInput, TNextStepResult>> trueBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, builder)
    {
        PreviousStep = previousStep;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler(Handler<TNextStepInput, TNextStepResult> handler)
    {
        var trueHandler = TruePipeline.BuildHandler(handler);
        var selector = CreateStepSelector();
        ValueTask<TResult> Handler(TInput input, CancellationToken ct) => selector(input, trueHandler, handler, ct);
        return PreviousStep.BuildHandler(Handler);
    }
}


