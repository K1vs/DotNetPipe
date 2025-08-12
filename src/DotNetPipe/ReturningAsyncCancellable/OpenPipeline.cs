using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps;
using System.Threading;

namespace K1vs.DotNetPipe.ReturningAsyncCancellable;

/// <summary>
/// Represents an open returning pipeline with cancellation support that can be used as a sub-pipeline
/// in if, ifelse or switch steps. It starts with an entry step and ends with a reduced pipe step.
/// </summary>
/// <typeparam name="TInput">The type of the input data that the pipeline processes.</typeparam>
/// <typeparam name="TInputResult">The type of the input result for the pipeline.</typeparam>
/// <typeparam name="TNextInput">The type of the next input data for the pipeline.</typeparam>
/// <typeparam name="TNextInputResult">The type of the next input result for the pipeline.</typeparam>
public class OpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult> : Pipeline
{
    /// <inheritdoc/>
    public override bool IsOpenPipeline => true;

    /// <summary>
    /// Gets the reduced pipe step that represents the last step in the pipeline.
    /// This step is used for integration of this pipeline in another pipeline as a sub-pipeline.
    /// </summary>
    public ReducedPipeStep<TInput, TInputResult, TNextInput, TNextInputResult> ReducedPipeStep { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenPipeline{TInput, TInputResult, TNextInput, TNextInputResult}"/> class.
    /// </summary>
    /// <param name="name">The name of the pipeline.</param>
    /// <param name="entryStep">The entry step of the pipeline.</param>
    /// <param name="lastStep">The last step (reduced pipe step) of the pipeline.</param>
    internal OpenPipeline(string name, Step entryStep, ReducedPipeStep<TInput, TInputResult, TNextInput, TNextInputResult> lastStep)
        : base(name, entryStep, lastStep)
    {
        ReducedPipeStep = lastStep;
    }

    /// <summary>
    /// Builds a handler for the pipeline using the provided handler for the next input type.
    /// </summary>
    /// <param name="handler">The handler for the next input type.</param>
    /// <returns>A handler that processes the input data and passes it to the next step.</returns>
    internal Handler<TInput, TInputResult> BuildHandler(Handler<TNextInput, TNextInputResult> handler)
    {
        return ReducedPipeStep.BuildHandler(handler);
    }
}



