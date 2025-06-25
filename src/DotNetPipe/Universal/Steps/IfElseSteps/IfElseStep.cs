using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Universal.Steps.IfElseSteps;

/// <summary>
/// Represents a step in a pipeline that conditionally executes one of two pipelines based on a selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of input for the if-else step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the true branch (if condition is true).</typeparam>
/// <typeparam name="TElseInput">The type of input for the false branch (if condition is false).</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the if-else step.</typeparam>
public abstract class IfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextStepInput> : ReducedPipeStep<TEntryStepInput, TNextStepInput>
{
    private readonly IfElseSelector<TInput, TIfInput, TElseInput> _selector;

    /// <summary>
    /// Mutators for the if-else selector.
    /// These allow for modifying the behavior of the selector dynamically.
    /// </summary>
    public StepMutators<IfElseSelector<TInput, TIfInput, TElseInput>> Mutators { get; }

    /// <summary>
    /// Gets the pipeline that is executed when the condition is true.
    /// </summary>
    public OpenPipeline<TIfInput, TNextStepInput> TruePipeline { get; }

    /// <summary>
    /// Gets the pipeline that is executed when the condition is false.
    /// </summary>
    public OpenPipeline<TElseInput, TNextStepInput> ElsePipeline { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IfElseStep{TEntryStepInput, TInput, TIfInput, TElseInput, TNextStepInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute.</param>
    /// <param name="trueBuilder">A function that builds the pipeline for the true branch.</param>
    /// <param name="elseBuilder">A function that builds the pipeline for the false branch.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    private protected IfElseStep(string name,
        IfElseSelector<TInput, TIfInput, TElseInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TNextStepInput>> elseBuilder,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        TruePipeline = trueBuilder(builder.Space);
        ElsePipeline = elseBuilder(builder.Space);
        Mutators = new StepMutators<IfElseSelector<TInput, TIfInput, TElseInput>>();
    }

    /// <summary>
    /// Creates a step selector that can be used to determine which pipeline to execute based on the input.
    /// This method applies any mutations defined in the Mutators collection to the original selector.
    /// </summary>
    /// <returns>A mutated version of the original selector.</returns>
    private protected IfElseSelector<TInput, TIfInput, TElseInput> CreateStepSelector()
    {
        return Mutators.MutateDelegate(_selector);
    }
}
