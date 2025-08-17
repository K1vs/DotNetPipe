namespace K1vs.DotNetPipe.ReturningSync.Steps.IfElseSteps;

/// <summary>
/// Represents a step inside a pipeline that conditionally executes one of two pipelines based on a selector.
/// This step is used inside a pipeline, after a previous step that produces an input.
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
public class PipeIfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult> : IfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult>
{
    /// <summary>
    /// Gets the previous step in the pipeline that this if-else step follows.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> PreviousStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeIfElseStep{TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult}"/> class.
    /// </summary>
    /// <param name="previousStep">The previous step in the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute.</param>
    /// <param name="trueBuilder">A function that builds the pipeline for the true branch.</param>
    /// <param name="elseBuilder">A function that builds the pipeline for the false branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal PipeIfElseStep(ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> previousStep,
        string name,
        IfElseSelector<TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepInput, TNextStepResult>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TElseResult, TNextStepInput, TNextStepResult>> elseBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, elseBuilder, builder)
    {
        PreviousStep = previousStep;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler(Handler<TNextStepInput, TNextStepResult> handler)
    {
        var selector = CreateStepSelector();
        var trueHandler = TruePipeline.BuildHandler(handler);
        var elseHandler = ElsePipeline.BuildHandler(handler);
        TResult Handler(TInput input) => selector(input, trueHandler, elseHandler);
        return PreviousStep.BuildHandler(Handler);
    }
}

