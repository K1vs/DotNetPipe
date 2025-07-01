namespace K1vs.DotNetPipe.AsyncCancellable.Steps.LinearSteps;

/// <summary>
/// Represents a linear step inside a pipeline that processes input and produces output.
/// This step is used to create a sequence of operations where each step processes the output of the previous step.
/// </summary>
/// <typeparam name="TEntryStepInput"></typeparam>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TNextInput"></typeparam>
public sealed class PipeLinearStep<TEntryStepInput, TInput, TNextInput> : LinearStep<TEntryStepInput, TInput, TNextInput>
{
    /// <summary>
    /// Gets the previous step in the pipeline that this linear step follows.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TInput> PreviousStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeLinearStep{TEntryStepInput, TInput, TNextInput}"/> class.
    /// </summary>
    /// <param name="previousStep">The previous step in the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <param name="delegate">The delegate that processes the input and produces the output.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal PipeLinearStep(ReducedPipeStep<TEntryStepInput, TInput> previousStep,
        string name,
        Pipe<TInput, TNextInput> @delegate,
        PipelineBuilder builder) : base(name, @delegate, builder)
    {
        PreviousStep = previousStep;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler(Handler<TNextInput> handler)
    {
        var pipe = CreateStepPipe();
        var resultHandler = PreviousStep.BuildHandler((input, ct) => pipe(input, handler, ct));
        return resultHandler;
    }
}
