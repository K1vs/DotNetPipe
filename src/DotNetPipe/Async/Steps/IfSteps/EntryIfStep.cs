namespace K1vs.DotNetPipe.Async.Steps.IfSteps;

/// <summary>
/// Represents a entry step in a pipeline that conditionally processes input based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if step.</typeparam>
public sealed class EntryIfStep<TEntryStepInput, TIfInput, TNextStepInput> : IfStep<TEntryStepInput, TEntryStepInput, TIfInput, TNextStepInput>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryIfStep{TEntryStepInput, TIfInput, TNextStepInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute when the condition is true.</param>
    /// <param name="trueBuilder">A function that builds the pipeline for the true branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal EntryIfStep(string name, IfSelector<TEntryStepInput, TIfInput, TNextStepInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, builder)
    {
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var trueHandler = TruePipeline.BuildHandler(handler);
        var selector = CreateStepSelector();
        Task Handler(TEntryStepInput input) => selector(input, trueHandler, handler);
        return Handler;
    }
}
