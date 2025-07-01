namespace K1vs.DotNetPipe.AsyncCancellable.Steps.SwitchSteps;

using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.AsyncCancellable;

/// <summary>
/// Represents a step inside a pipeline that processes input and routes it to different cases based on a selector.
/// If no case matches, it routes to a default pipeline.
/// After branching, it continues to the next step in the pipeline.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the switch step.</typeparam>
/// <typeparam name="TCaseInput">The type of input for the case branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the switch step.</typeparam>
public abstract class SwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> : ReducedPipeStep<TEntryStepInput, TNextStepInput>
{
    private readonly SwitchSelector<TInput, TCaseInput, TDefaultInput> _selector;

    /// <summary>
    /// Mutators for the switch selector.
    /// These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<SwitchSelector<TInput, TCaseInput, TDefaultInput>> Mutators { get; }

    /// <summary>
    /// Gets the pipelines for the case branches of the switch step.
    /// Each case is identified by a string key, and the value is the pipeline that processes that case's input.
    /// </summary>
    public IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextStepInput>> CasesPipelines { get; }

    /// <summary>
    /// Gets the default pipeline of the switch step.
    /// This pipeline is executed when no case matches the input.
    /// </summary>
    public OpenPipeline<TDefaultInput, TNextStepInput> DefaultPipeline { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchStep{TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which case to execute based on the input.</param>
    /// <param name="caseBuilder">A function that builds the pipelines for the case branches.</param>
    /// <param name="defaultPipeline">The pipeline that is executed when no case matches the input.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    private protected SwitchStep(string name,
        SwitchSelector<TInput, TCaseInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextStepInput>>> caseBuilder,
        OpenPipeline<TDefaultInput, TNextStepInput> defaultPipeline,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<SwitchSelector<TInput, TCaseInput, TDefaultInput>>();
        CasesPipelines = caseBuilder(builder.Space);
        DefaultPipeline = defaultPipeline;
    }

    /// <summary>
    /// Creates a step selector that can be used to determine which pipeline to execute based on the input.
    /// This method applies any mutations defined in the Mutators collection to the original selector.
    /// </summary>
    /// <returns>A mutated version of the original selector.</returns>
    private protected SwitchSelector<TInput, TCaseInput, TDefaultInput> CreateStepSelector() => Mutators.MutateDelegate(_selector);
}