namespace K1vs.DotNetPipe.Async.Steps.HandlerSteps;

/// <summary>
/// Represents a step in a pipeline that starts and ends (handler input) pipeline.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
public sealed class EntryHandlerStep<TEntryStepInput> : HandlerStep<TEntryStepInput, TEntryStepInput>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryHandlerStep{TEntryStepInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="handler">The handler that processes the input.</param>
    /// <param name="builder">The pipeline builder.</param>
    internal EntryHandlerStep(string name, Handler<TEntryStepInput> handler, PipelineBuilder builder)
        : base(name, handler, builder)
    {
    }

    /// <inheritdoc/>
    public override Pipeline<TEntryStepInput> BuildPipeline()
    {
        if (Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        var pipeline = new Pipeline<TEntryStepInput>(Builder.Name, Builder.EntryStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler()
    {
        var handler = CreateStepHandler();
        return handler;
    }
}
