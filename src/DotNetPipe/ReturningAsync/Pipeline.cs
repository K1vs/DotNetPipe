using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.ReturningAsync.Steps;

namespace K1vs.DotNetPipe.ReturningAsync;

/// <summary>
/// Represents a pipeline in the DotNetPipe framework.
/// A pipeline consists of a series of steps that process input data sequentially.
/// Each pipeline has an entry step where processing begins and a last step that concludes the processing.
/// Pipelines can be open, allowing integrate it in another pipeline as a sub-pipeline in if, ifelse or switch step.
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
    /// True if the pipeline is an open pipeline, meaning it can be extended with additional steps.
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
/// Represents a pipeline that processes input of type <typeparamref name="TInput"/>.
/// A pipeline can consist of multiple steps, where each step can perform operations on the input data.
/// Pipelines typically start with an entry step and end with a handler step.
/// But can also be short pipelines with has entry step and handler step as the same step.
/// Also can exist step with fork step at the end without handler step, but in this case handler step will be existed in fork branches.
/// This pipeline itself can be used as a sub-pipeline in another pipeline in fork or multifork step.
/// </summary>
/// <typeparam name="TInput">The type of the input data that the pipeline processes.</typeparam>
/// <typeparam name="TInputResult">The type of the result produced by the pipeline.</typeparam>
public class Pipeline<TInput, TInputResult> : Pipeline
{
    private readonly Func<Handler<TInput, TInputResult>> _buildHandler;

    /// <inheritdoc/>
    public override bool IsOpenPipeline => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pipeline{TInput, TInputResult}"/> class.
    /// </summary>
    /// <param name="name">The name of the pipeline.</param>
    /// <param name="entryStep">The entry step of the pipeline.</param>
    /// <param name="lastStep">The last step of the pipeline.</param>
    /// <param name="buildHandler">A function that builds the handler for the pipeline.</param>
    internal Pipeline(string name, Step entryStep, Step lastStep, Func<Handler<TInput, TInputResult>> buildHandler)
        : base(name, entryStep, lastStep)
    {
        _buildHandler = buildHandler;
    }

    /// <summary>
    /// Compiles the pipeline into a handler that can be executed.
    /// Optionally, you can configure mutators for the pipeline, using the provided <paramref name="mutatorsConfigurator"/>.
    /// The mutators can modify the behavior of the steps in the pipeline, allowing for dynamic changes to how the pipeline processes data.
    /// </summary>
    /// <param name="mutatorsConfigurator">An optional action to configure mutators for the pipeline.</param>
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

