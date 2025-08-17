using K1vs.DotNetPipe.ReturningSync.Steps;

namespace K1vs.DotNetPipe.ReturningSync;

/// <summary>
/// Represents an open pipeline that can used as a sub-pipeline in another pipeline in if, ifelse or switch step.
/// An open pipeline starts with an entry step and ends with a reduced pipe step, which is used to integrate it into another pipeline.
/// </summary>
/// <typeparam name="TInput">The type of the input data that the pipeline processes.</typeparam>
/// <typeparam name="TInputResult">The type of the result produced by the pipeline.</typeparam>
/// <typeparam name="TNextInput">The type of the next input data that the pipeline processes after the entry step.</typeparam>
/// <typeparam name="TNextInputResult">The type of the result produced by the next step.</typeparam>
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
    /// <param name="lastStep">The last step of the pipeline, which is a reduced pipe step.</param>
    internal OpenPipeline(string name, Step entryStep, ReducedPipeStep<TInput, TInputResult, TNextInput, TNextInputResult> lastStep)
        : base(name, entryStep, lastStep)
    {
        ReducedPipeStep = lastStep;
    }

    /// <summary>
    /// Builds a handler for the pipeline using the provided handler for the next input type.
    /// This is a main way to integrate this open pipeline in another pipeline as a sub-pipeline.
    /// </summary>
    /// <param name="handler">The handler for the next input type.</param>
    /// <returns>A handler that processes the input data and passes it to the next step in the pipeline.</returns>
    internal Handler<TInput, TInputResult> BuildHandler(Handler<TNextInput, TNextInputResult> handler)
    {
        return ReducedPipeStep.BuildHandler(handler);
    }
}

