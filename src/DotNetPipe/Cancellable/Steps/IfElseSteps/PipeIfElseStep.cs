namespace K1vs.DotNetPipe.Cancellable.Steps.IfElseSteps;

/// <summary>
/// Represents a step inside a pipeline that conditionally executes one of two pipelines based on a selector.
/// This step is used inside a pipeline, after a previous step that produces an input.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of input for the if-else step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TElseInput">The type of input for the false branch (if condition is false).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if-else step.</typeparam>
public class PipeIfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextStepInput> : IfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextStepInput>
{
    /// <summary>
    /// Gets the previous step in the pipeline that this if-else step follows.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TInput> PreviousStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeIfElseStep{TEntryStepInput, TInput, TIfInput, TElseInput, TNextStepInput}"/> class.
    /// </summary>
    /// <param name="previousStep">The previous step in the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute.</param>
    /// <param name="trueBuilder">A function that builds the pipeline for the true branch.</param>
    /// <param name="elseBuilder">A function that builds the pipeline for the false branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal PipeIfElseStep(ReducedPipeStep<TEntryStepInput, TInput> previousStep,
        string name,
        IfElseSelector<TInput, TIfInput, TElseInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TNextStepInput>> elseBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, elseBuilder, builder)
    {
        PreviousStep = previousStep;
    }

    /// <summary>
    /// Builds the pipeline that includes this if-else step.
    /// </summary>
    /// <returns>A pipeline that starts with the entry step input and ends with this if-else step.</returns>
    internal override Handler<TEntryStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var selector = CreateStepSelector();
        var trueHandler = TruePipeline.BuildHandler(handler);
        var elseHandler = ElsePipeline.BuildHandler(handler);
        ValueTask Handler(TInput input, CancellationToken ct) => selector(input, trueHandler, elseHandler, ct);
        return PreviousStep.BuildHandler(Handler);
    }
}
