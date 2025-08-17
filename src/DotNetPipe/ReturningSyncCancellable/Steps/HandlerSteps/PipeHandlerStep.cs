namespace K1vs.DotNetPipe.ReturningSyncCancellable.Steps.HandlerSteps;

/// <summary>
/// Represents a step pipeline that ends pipeline with a handler.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the handler.</typeparam>
/// <typeparam name="TResult">The type of the result for the handler.</typeparam>
public sealed class PipeHandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult> : HandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult>
{
    /// <summary>
    /// Gets the previous step in the pipeline that this handler step follows.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> PreviousStep { get; }

    internal PipeHandlerStep(ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> previous,
        string name,
        Handler<TInput, TResult> handler,
        PipelineBuilder builder)
        : base(name, handler, builder)
    {
        PreviousStep = previous;
    }

    /// <inheritdoc/>
    public override Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline()
    {
        if (PreviousStep is null)
        {
            throw new InvalidOperationException("Previous step is not set");
        }
        var pipeline = new Pipeline<TEntryStepInput, TEntryStepResult>(Builder.Name, PreviousStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler()
    {
        var handler = CreateStepHandler();
        var pipe = PreviousStep.BuildHandler(handler);
        return pipe;
    }
}



