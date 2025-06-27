namespace K1vs.DotNetPipe.Async.Steps.SwitchSteps;

/// <summary>
/// Represents a entry switch step in a start of pipeline that allows branching based on a selector.
/// It processes input and routes it to different cases based on the selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TCaseInput">The type of input for the case branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the switch step.</typeparam>
public class EntrySwitchStep<TEntryStepInput, TCaseInput, TDefaultInput, TNextStepInput> : SwitchStep<TEntryStepInput, TEntryStepInput, TCaseInput, TDefaultInput, TNextStepInput>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntrySwitchStep{TEntryStepInput, TCaseInput, TDefaultInput, TNextStepInput}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that determines which pipeline to execute based on the input.</param>
    /// <param name="caseBuilder">A function that builds the pipelines for the case branches.</param>
    /// <param name="defaultPipeline">The pipeline that is executed when no case matches the input.</param>
    /// <param name="builder">The pipeline builder that manages the pipeline construction.</param>
    internal EntrySwitchStep(string name,
        SwitchSelector<TEntryStepInput, TCaseInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextStepInput>>> caseBuilder,
        OpenPipeline<TDefaultInput, TNextStepInput> defaultPipeline,
        PipelineBuilder builder)
        : base(name, selector, caseBuilder, defaultPipeline, builder)
    {
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var selector = CreateStepSelector();
        var casesHandlers = CasesPipelines.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.BuildHandler(handler)).AsReadOnly();
        var defaultHandler = DefaultPipeline.BuildHandler(handler);
        Task Handler(TEntryStepInput input) => selector(input, casesHandlers, defaultHandler);
        return Handler;
    }
}
