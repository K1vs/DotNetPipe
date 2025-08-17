using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.ReturningSyncCancellable.Steps;

namespace K1vs.DotNetPipe.ReturningSyncCancellable;

/// <summary>
/// Represents a pipeline in the DotNetPipe framework for returning pipelines with cancellation support.
/// A pipeline consists of a series of steps that process input data sequentially.
/// Each pipeline has an entry step where processing begins and a last step that concludes the processing.
/// Pipelines can be open, allowing integration as a sub-pipeline in if, ifelse or switch step.
/// Otherwise, it can be called directly or can be integrated in another pipeline as a sub-pipeline in fork or multifork step.
/// </summary>
public abstract class Pipeline
{
    /// <summary>
    /// Name of the pipeline.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Entry step of the pipeline.
    /// </summary>
    public Step EntryStep { get; }
    /// <summary>
    /// Last step of the pipeline.
    /// If the pipeline is open, this is the last step that can be extended.
    /// If the pipeline is closed, this is the step that handles the final input, e.g. a handler step.
    /// </summary>
    public Step LastStep { get; }
        /// <summary>
        /// Gets a value indicating whether the pipeline is open (can be extended with additional steps).
        /// </summary>
    public abstract bool IsOpenPipeline { get; }
    /// <summary>
    /// Gets the space in which the pipeline is defined.
    /// </summary>
    public Space Space => EntryStep.Builder.Space;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pipeline"/> class.
    /// </summary>
    /// <param name="name">The name of the pipeline.</param>
    /// <param name="entryStep">The entry step of the pipeline.</param>
    /// <param name="lastStep">The last step of the pipeline.</param>
    public Pipeline(string name, Step entryStep, Step lastStep)
    {
        Name = name;
        EntryStep = entryStep;
        LastStep = lastStep;
    }
}

    /// <summary>
    /// Represents a pipeline that processes input of type <typeparamref name="TInput"/> and returns <typeparamref name="TInputResult"/>.
    /// Can be compiled into a handler delegate and used as a sub-pipeline.
    /// </summary>
public class Pipeline<TInput, TInputResult> : Pipeline
{
    private readonly Func<Handler<TInput, TInputResult>> _buildHandler;

    /// <inheritdoc/>
    public override bool IsOpenPipeline => false;

    internal Pipeline(string name, Step entryStep, Step lastStep, Func<Handler<TInput, TInputResult>> buildHandler)
        : base(name, entryStep, lastStep)
    {
        _buildHandler = buildHandler;
    }

        /// <summary>
        /// Compiles the pipeline into a handler delegate.
        /// </summary>
        /// <param name="mutatorsConfigurator">An optional configurator for mutators that can be used to modify the pipeline.</param>
        /// <returns>A handler delegate that can be used to process input data.</returns>
    public Handler<TInput, TInputResult> Compile(Action<MutatorsConfigurator<Space>>? mutatorsConfigurator = null)
    {
        if (mutatorsConfigurator != null)
        {
            var configurator = new MutatorsConfigurator<Space>();
            mutatorsConfigurator?.Invoke(configurator);
            configurator.RegisterMutators(Space);
        }
        return _buildHandler();
    }
}



