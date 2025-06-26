using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Sync.Steps.HandlerSteps;

/// <summary>
/// Represents a step that contains a handler.
/// This step is last step in the pipeline and is used to process transformed by previous steps input.
/// </summary>
/// <typeparam name="TEntryStepInput">Type of input for the entry step in the pipeline.</typeparam>
/// <typeparam name="TInput">Type of input for the handler.</typeparam>
public abstract class HandlerStep<TEntryStepInput, TInput> : Step
{
    private readonly Handler<TInput> _originalHandler;

    /// <summary>
    /// Mutators for the handler.
    /// These allow for modifying the behavior of the handler dynamically.
    /// </summary>
    public StepMutators<Handler<TInput>> Mutators { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerStep{TEntryStepInput, TInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="handler">The handler that processes the input.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    private protected HandlerStep(string name, Handler<TInput> handler, PipelineBuilder builder)
        : base(name, builder)
    {
        _originalHandler = handler;
        Mutators = new StepMutators<Handler<TInput>>();
    }

    /// <summary>
    /// Builds the pipeline that ends by this handler step.
    /// </summary>
    /// <returns>A pipeline that starts with the entry step input and ends with this handler step.</returns>
    public abstract Pipeline<TEntryStepInput> BuildPipeline();

    /// <summary>
    /// Builds the handler for this step.
    /// </summary>
    /// <returns>A handler that processes the input and performs the defined operations.</returns>
    internal abstract Handler<TEntryStepInput> BuildHandler();

    /// <summary>
    /// Creates a step handler that can be used to process the input.
    /// This method applies any mutations defined in the Mutators collection to the original handler.
    /// </summary>
    /// <returns>A mutated version of the original handler.</returns>
    private protected Handler<TInput> CreateStepHandler()
    {
        var handler = Mutators.MutateDelegate(_originalHandler);
        return handler;
    }
}
