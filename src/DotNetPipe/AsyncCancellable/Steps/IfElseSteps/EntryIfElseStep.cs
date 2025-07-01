namespace K1vs.DotNetPipe.AsyncCancellable.Steps.IfElseSteps;

/// <summary>
/// Represents a entry step in a pipeline that conditionally processes input based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TElseInput">The type of input for the false branch (if condition is false).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if-else step.</typeparam>
public class EntryIfElseStep<TEntryStepInput, TIfInput, TElseInput, TNextStepInput> : IfElseStep<TEntryStepInput, TEntryStepInput, TIfInput, TElseInput, TNextStepInput>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryIfElseStep{TEntryStepInput, TIfInput, TElseInput, TNextStepInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute.</param>
    /// <param name="trueBuilder">A function that builds the pipeline for the true branch.</param>
    /// <param name="elseBuilder">A function that builds the pipeline for the false branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal EntryIfElseStep(string name, IfElseSelector<TEntryStepInput, TIfInput, TElseInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TNextStepInput>> elseBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, elseBuilder, builder)
    {
    }

    /// <summary>
    /// Builds the handler for this entry if-else step.
    /// </summary>
    /// <param name="handler">The handler for the next step in the pipeline.</param>
    /// <returns>A handler that processes the input and determines the next step based on the condition.</returns>
    internal override Handler<TEntryStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var selector = CreateStepSelector();
        var trueHandler = TruePipeline.BuildHandler(handler);
        var elseHandler = ElsePipeline.BuildHandler(handler);
        Task Handler(TEntryStepInput input, CancellationToken ct) => selector(input, trueHandler, elseHandler, ct);
        return Handler;
    }
}
