using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Async.Steps;

namespace K1vs.DotNetPipe.Async;

/// <summary>
/// Represents a pipeline that processes input of type <typeparamref name="TInput"/>.
/// A pipeline can consist of multiple steps, where each step can perform operations on the input data.
/// Pipelines typically start with an entry step and end with a handler step.
/// But can also be short pipelines with has entry step and handler step as the same step.
/// Also can exist step with fork step at the end without handler step, but in this case handler step will be existed in fork branches.
/// This pipeline itself can be used as a sub-pipeline in another pipeline in fork or multifork step.
/// </summary>
/// <typeparam name="TInput">The type of the input data that the pipeline processes.</typeparam>
public class Pipeline<TInput> : IPipeline
{
    private readonly Func<Handler<TInput>> _buildHandler;

    /// <summary>
    /// Gets the space in which the pipeline is defined.
    /// </summary>
    public Space Space => EntryStep.Builder.Space;

    /// <summary>
    /// Gets the name of the pipeline.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc/>
    public Step EntryStep { get; }

    /// <inheritdoc/>
    public Step LastStep { get; }

    /// <inheritdoc/>
    public bool IsOpenPipeline => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pipeline{TInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the pipeline.</param>
    /// <param name="entryStep">The entry step of the pipeline.</param>
    /// <param name="lastStep">The last step of the pipeline.</param>
    /// <param name="buildHandler">A function that builds the handler for the pipeline.</param>
    internal Pipeline(string name, Step entryStep, Step lastStep, Func<Handler<TInput>> buildHandler)
    {
        Name = name;
        EntryStep = entryStep;
        LastStep = lastStep;
        _buildHandler = buildHandler;
    }

    /// <summary>
    /// Compiles the pipeline into a handler that can be executed.
    /// Optionally, you can configure mutators for the pipeline, using the provided <see cref="mutatorsConfigurator"/>.
    /// The mutators can modify the behavior of the steps in the pipeline, allowing for dynamic changes to how the pipeline processes data.
    /// </summary>
    /// <param name="mutatorsConfigurator">An optional action to configure mutators for the pipeline.</param>
    public Handler<TInput> Compile(Action<MutatorsConfigurator<Space>>? mutatorsConfigurator = null)
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
