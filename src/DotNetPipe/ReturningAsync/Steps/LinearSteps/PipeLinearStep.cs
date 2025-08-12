namespace K1vs.DotNetPipe.ReturningAsync.Steps.LinearSteps;

/// <summary>
/// Represents a linear step inside a pipeline that processes input and produces output.
/// This step is used to create a sequence of operations where each step processes the output of the previous step.
/// </summary>
/// <typeparam name="TEntryStepInput"></typeparam>
/// <typeparam name="TEntryStepResult"></typeparam>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TResult"></typeparam>
/// <typeparam name="TNextInput"></typeparam>
/// <typeparam name="TNextResult"></typeparam>
public sealed class PipeLinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult> :
    LinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult>
{
    /// <summary>
    /// Gets the previous step in the pipeline that this linear step follows.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> PreviousStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeLinearStep{TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult}"/> class.
    /// </summary>
    /// <param name="previousStep">The previous step in the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <param name="delegate">The delegate that processes the input and produces the output.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal PipeLinearStep(ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> previousStep,
        string name,
        Pipe<TInput, TResult, TNextInput, TNextResult> @delegate,
        PipelineBuilder builder) : base(name, @delegate, builder)
    {
        PreviousStep = previousStep;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler(Handler<TNextInput, TNextResult> handler)
    {
        var pipe = CreateStepPipe();
        var resultHandler = PreviousStep.BuildHandler((input) => pipe(input, handler));
        return resultHandler;
    }
}

