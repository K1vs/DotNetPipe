namespace K1vs.DotNetPipe.Universal.Steps.HandlerSteps;

/// <summary>
/// Represents a step pipeline a pipeline that ends pipeline with a handler.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the handler.</typeparam>
public sealed class PipeHandlerStep<TEntryStepInput, TInput> : HandlerStep<TEntryStepInput, TInput>
{
    /// <summary>
    /// Gets the previous step in the pipeline that this handler step follows.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TInput> PreviousStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeHandlerStep{TEntryStepInput, TInput}"/> class.
    /// </summary>
    /// <param name="previous">The previous step in the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <param name="handler">The handler that processes the input.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal PipeHandlerStep(ReducedPipeStep<TEntryStepInput, TInput> previous,
        string name,
        Handler<TInput> handler,
        PipelineBuilder builder)
        : base(name, handler, builder)
    {
        PreviousStep = previous;
    }

    /// <inheritdoc/>
    public override Pipeline<TEntryStepInput> BuildPipeline()
    {
        if (PreviousStep is null)
        {
            throw new InvalidOperationException("Previous step is not set");
        }
        var pipeline = new Pipeline<TEntryStepInput>(Builder.Name, PreviousStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler()
    {
        var handler = CreateStepHandler();
        var pipe = PreviousStep.BuildHandler(handler);
        return pipe;
    }
}
