namespace K1vs.DotNetPipe.ReturningSyncCancellable.Steps.HandlerSteps;

/// <summary>
/// Represents a step in a pipeline that starts and ends (handler input) pipeline.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
public sealed class EntryHandlerStep<TEntryStepInput, TEntryStepResult> : HandlerStep<TEntryStepInput, TEntryStepResult, TEntryStepInput, TEntryStepResult>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    internal EntryHandlerStep(string name, Handler<TEntryStepInput, TEntryStepResult> handler, PipelineBuilder builder)
        : base(name, handler, builder)
    {
    }

    /// <inheritdoc/>
    public override Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline()
    {
        if (Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        var pipeline = new Pipeline<TEntryStepInput, TEntryStepResult>(Builder.Name, Builder.EntryStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler()
    {
        var handler = CreateStepHandler();
        return handler;
    }
}



