namespace K1vs.DotNetPipe.Async.Steps.LinearSteps;

/// <summary>
/// Represents a entry linear step in a start of pipeline that processes input and produces output.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TNextInput">The type of the input for the next step after this step.</typeparam>
public sealed class EntryLinearStep<TEntryStepInput, TNextInput> : LinearStep<TEntryStepInput, TEntryStepInput, TNextInput>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryLinearStep{TEntryStepInput, TNextInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="originalPipe">The original pipe that processes the input and produces the output.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal EntryLinearStep(string name, Pipe<TEntryStepInput, TNextInput> originalPipe, PipelineBuilder builder)
        : base(name, originalPipe, builder)
    {
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler(Handler<TNextInput> handler)
    {
        var pipe = CreateStepPipe();
        return (input) => pipe(input, handler);
    }
}
