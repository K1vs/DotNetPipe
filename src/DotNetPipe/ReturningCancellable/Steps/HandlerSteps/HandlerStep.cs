using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.ReturningCancellable.Steps.HandlerSteps;

/// <summary>
/// Represents a step that contains a handler. This step is last step in the pipeline and is used to process transformed input.
/// </summary>
/// <typeparam name="TEntryStepInput">Type of input for the entry step in the pipeline.</typeparam>
/// <typeparam name="TEntryStepResult">Type of result for the entry step in the pipeline.</typeparam>
/// <typeparam name="TInput">Type of input for the handler.</typeparam>
/// <typeparam name="TResult">Type of result for the handler.</typeparam>
public abstract class HandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult> : Step
{
    private readonly Handler<TInput, TResult> _originalHandler;
    /// <summary>
    /// Mutators for the handler. These allow for modifying the behavior of the handler dynamically.
    /// </summary>
    public StepMutators<Handler<TInput, TResult>> Mutators { get; }

    private protected HandlerStep(string name, Handler<TInput, TResult> handler, PipelineBuilder builder)
        : base(name, builder)
    {
        _originalHandler = handler;
        Mutators = new StepMutators<Handler<TInput, TResult>>();
    }

    /// <summary>
    /// Builds the pipeline that ends by this handler step.
    /// </summary>
    public abstract Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline();
    /// <summary>
    /// Builds the handler for this step.
    /// </summary>
    internal abstract Handler<TEntryStepInput, TEntryStepResult> BuildHandler();

    /// <summary>
    /// Creates a step handler that can be used to process the input.
    /// This method applies any mutations defined in the Mutators collection to the original handler.
    /// </summary>
    private protected Handler<TInput, TResult> CreateStepHandler()
    {
        var handler = Mutators.MutateDelegate(_originalHandler);
        return handler;
    }
}


