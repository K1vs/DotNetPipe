namespace K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.IfElseSteps;

/// <summary>
/// Represents a entry step in a pipeline that conditionally processes input based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TResult">The type of the result for the if-else step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TIfResult">The type of result for the true branch (if condition is true).</typeparam>
/// <typeparam name="TElseInput">The type of input for the false branch (if condition is false).</typeparam>
/// <typeparam name="TElseResult">The type of the result for the false branch (if condition is false).</typeparam>
/// <typeparam name="TNextStepInput">The type of the input for the next step after the if-else step.</typeparam>
/// <typeparam name="TNextStepResult">The type of the result for the next step after the if-else step.</typeparam>
public class EntryIfElseStep<TEntryStepInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult> : IfElseStep<TEntryStepInput, TResult, TEntryStepInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    internal EntryIfElseStep(string name,
        IfElseSelector<TEntryStepInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepInput, TNextStepResult>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TElseResult, TNextStepInput, TNextStepResult>> elseBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, elseBuilder, builder)
    {
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TResult> BuildHandler(Handler<TNextStepInput, TNextStepResult> handler)
    {
        var selector = CreateStepSelector();
        var trueHandler = TruePipeline.BuildHandler(handler);
        var elseHandler = ElsePipeline.BuildHandler(handler);
        Task<TResult> Handler(TEntryStepInput input, CancellationToken ct) => selector(input, trueHandler, elseHandler, ct);
        return Handler;
    }
}



