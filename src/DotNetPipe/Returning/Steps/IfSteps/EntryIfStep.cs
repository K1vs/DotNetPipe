namespace K1vs.DotNetPipe.Returning.Steps.IfSteps;

/// <summary>
/// Represents a entry step in a pipeline that conditionally processes input based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TResult">The type of the result for the if step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TIfResult">The type of result for the true branch (if condition is true).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if step.</typeparam>
/// <typeparam name="TNextStepResult">The type of result for the next step after the if step.</typeparam>
public sealed class EntryIfStep<TEntryStepInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult> : IfStep<TEntryStepInput, TResult, TEntryStepInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryIfStep{TEntryStepInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute when the condition is true.</param>
    /// <param name="trueBuilder">A function that builds the pipeline for the true branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal EntryIfStep(string name, IfSelector<TEntryStepInput, TIfInput, TNextStepInput, TResult, TIfResult, TNextStepResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepInput, TNextStepResult>> trueBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, builder)
    {
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TResult> BuildHandler(Handler<TNextStepInput, TNextStepResult> handler)
    {
        var trueHandler = TruePipeline.BuildHandler(handler);
        var selector = CreateStepSelector();
        ValueTask<TResult> Handler(TEntryStepInput input) => selector(input, trueHandler, handler);
        return Handler;
    }
}
