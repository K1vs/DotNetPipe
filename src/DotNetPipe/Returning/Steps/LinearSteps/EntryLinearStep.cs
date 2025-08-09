namespace K1vs.DotNetPipe.Returning.Steps.LinearSteps;

/// <summary>
/// Represents a entry linear step in a start of pipeline that processes input and produces output.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result produced by the entry step.</typeparam>
/// <typeparam name="TNextInput">The type of the input for the next step after this step.</typeparam>
/// <typeparam name="TNextResult">The type of the result produced by the next step after this step.</typeparam>
public sealed class EntryLinearStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult> :
    LinearStep<TEntryStepInput, TEntryStepResult, TEntryStepInput, TEntryStepResult, TNextInput, TNextResult>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryLinearStep{TEntryStepInput, TEntryStepResult, TNextInput, TNextResult}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="originalPipe">The original pipe that processes the input and produces the output.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal EntryLinearStep(string name, Pipe<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult> originalPipe, PipelineBuilder builder)
        : base(name, originalPipe, builder)
    {
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler(Handler<TNextInput, TNextResult> handler)
    {
        var pipe = CreateStepPipe();
        return (input) => pipe(input, handler);
    }
}
