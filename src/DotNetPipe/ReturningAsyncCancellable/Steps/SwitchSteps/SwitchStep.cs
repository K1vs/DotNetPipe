using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.SwitchSteps;

/// <summary>
/// Represents a step inside a pipeline that processes input and routes it to different cases based on a selector.
/// If no case matches, it routes to a default pipeline. After branching, it continues to the next step in the pipeline.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the switch step.</typeparam>
/// <typeparam name="TResult">The type of the result for the switch step.</typeparam>
/// <typeparam name="TCaseInput">The type of input for the case branches.</typeparam>
/// <typeparam name="TCaseResult">The type of result for the case branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TDefaultResult">The type of result for the default branch.</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the switch step.</typeparam>
/// <typeparam name="TNextStepResult">The type of result for the next step after the switch step.</typeparam>
public abstract class SwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> : ReducedPipeStep<TEntryStepInput, TEntryStepResult, TNextStepInput, TNextStepResult>
{
    private readonly SwitchSelector<TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult> _selector;
    /// <summary>
    /// Mutators for the switch selector. These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<SwitchSelector<TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult>> Mutators { get; }
    /// <summary>
    /// Gets the pipelines for the case branches of the switch step.
    /// </summary>
    public IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextStepInput, TNextStepResult>> CasesPipelines { get; }
    /// <summary>
    /// Gets the default pipeline of the switch step.
    /// </summary>
    public OpenPipeline<TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> DefaultPipeline { get; }

    private protected SwitchStep(string name,
        SwitchSelector<TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextStepInput, TNextStepResult>>> caseBuilder,
        OpenPipeline<TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> defaultPipeline,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        Mutators = new StepMutators<SwitchSelector<TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult>>();
        CasesPipelines = caseBuilder(builder.Space);
        DefaultPipeline = defaultPipeline;
    }

    /// <summary>
    /// Creates a step selector that can be used to determine which pipeline to execute based on the input.
    /// This method applies any mutations defined in the Mutators collection to the original selector.
    /// </summary>
    /// <returns>A mutated version of the original selector.</returns>
    private protected SwitchSelector<TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult> CreateStepSelector() => Mutators.MutateDelegate(_selector);
}



