using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.ForkSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.HandlerSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.IfElseSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.IfSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.LinearSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.MultiForkSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.SwitchSteps;

namespace K1vs.DotNetPipe.ReturningAsyncCancellable;

/// <summary>
/// PipelineEntry is the entry point for building a returning cancellable pipeline.
/// It allows you to define the first step of the pipeline.
/// </summary>
/// <typeparam name="TPipelineInput">The type of the input data for the pipeline.</typeparam>
/// <typeparam name="TPipelineResult">The type of the result produced by the pipeline.</typeparam>
public class PipelineEntry<TPipelineInput, TPipelineResult>
{
    /// <summary>
    /// Gets the builder that is used to construct the pipeline.
    /// </summary>
    public PipelineBuilder Builder { get; }

    internal PipelineEntry(PipelineBuilder builder)
    {
        Builder = builder;
    }

    /// <summary>
    /// Starts a new linear step in the pipeline with the specified name and next step.
    /// </summary>
    public EntryLinearStep<TPipelineInput, TPipelineResult, TNextInput, TNextResult> StartWithLinear<TNextInput, TNextResult>(
        string name,
        Pipe<TPipelineInput, TPipelineResult, TNextInput, TNextResult> next)
    {
        var step = new EntryLinearStep<TPipelineInput, TPipelineResult, TNextInput, TNextResult>(name, next, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new linear step using provided step implementation.
    /// </summary>
    public EntryLinearStep<TPipelineInput, TPipelineResult, TNextInput, TNextResult> StartWithLinear<TNextInput, TNextResult>(
        ILinearStep<TPipelineInput, TPipelineResult, TNextInput, TNextResult> step)
    {
        return StartWithLinear<TNextInput, TNextResult>(step.Name, (input, next, ct) => step.Handle(input, next, ct));
    }

    /// <summary>
    /// Starts a new if step in the pipeline with the specified name and selector.
    /// </summary>
    public EntryIfStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult> StartWithIf<TIfInput, TIfResult, TNextInput, TNextResult>(
        string name,
        IfSelector<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextInput, TNextResult>> trueBuilder)
    {
        var step = new EntryIfStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult>(name, selector, trueBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new if step using provided step implementation.
    /// </summary>
    public EntryIfStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult> StartWithIf<TIfInput, TIfResult, TNextInput, TNextResult>(
        IIfStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult> step)
    {
        return StartWithIf<TIfInput, TIfResult, TNextInput, TNextResult>(step.Name, (input, ifNext, next, ct) => step.Handle(input, ifNext, next, ct), step.BuildTruePipeline);
    }

    /// <summary>
    /// Starts a new if-else step in the pipeline with the specified name and selector.
    /// </summary>
    public EntryIfElseStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult> StartWithIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult>(
        string name,
        IfElseSelector<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextInput, TNextResult>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TElseResult, TNextInput, TNextResult>> falseBuilder)
    {
        var step = new EntryIfElseStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult>(name, selector, trueBuilder, falseBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new if-else step using provided step implementation.
    /// </summary>
    public EntryIfElseStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult> StartWithIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult>(
        IIfElseStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult> step)
    {
        return StartWithIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult>(step.Name, (input, ifNext, elseNext, ct) => step.Handle(input, ifNext, elseNext, ct), step.BuildTruePipeline, step.BuildFalsePipeline);
    }

    /// <summary>
    /// Starts a new switch step in the pipeline with the specified name and selector.
    /// </summary>
    public EntrySwitchStep<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult> StartWithSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult>(
        string name,
        SwitchSelector<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextInput, TNextResult>>> caseBuilder,
        OpenPipeline<TDefaultInput, TDefaultResult, TNextInput, TNextResult> defaultBuilder)
    {
        var step = new EntrySwitchStep<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult>(name, selector, caseBuilder, defaultBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new switch step using provided step implementation.
    /// </summary>
    public EntrySwitchStep<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult> StartWithSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult>(
        ISwitchStep<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult> step)
    {
        return StartWithSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult>(step.Name, (input, cases, defaultNext, ct) => step.Handle(input, cases, defaultNext, ct), step.BuildCasesPipelines, step.BuildDefaultPipeline(Builder.Space));
    }

    /// <summary>
    /// Starts a new fork step in the pipeline with the specified name and selector.
    /// </summary>
    public EntryForkStep<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> StartWithFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(
        string name,
        ForkSelector<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> selector,
        Func<Space, Pipeline<TBranchAInput, TBranchAResult>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput, TBranchBResult>> branchBBuilder)
    {
        var step = new EntryForkStep<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(name, selector, branchABuilder, branchBBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new fork step using provided step implementation.
    /// </summary>
    public EntryForkStep<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> StartWithFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(
        IForkStep<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> step)
    {
        return StartWithFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(step.Name, (input, a, b, ct) => step.Handle(input, a, b, ct), step.BuildBranchAPipeline, step.BuildBranchBPipeline);
    }

    /// <summary>
    /// Starts a new multi-fork step in the pipeline with the specified name and selector.
    /// </summary>
    public EntryMultiForkStep<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> StartWithMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(
        string name,
        MultiForkSelector<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBuilder)
    {
        var step = new EntryMultiForkStep<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(name, selector, branchesBuilder, defaultBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new multi-fork step using provided step implementation.
    /// </summary>
    public EntryMultiForkStep<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> StartWithMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(
        IMultiForkStep<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> step)
    {
        return StartWithMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(step.Name, (input, branches, @default, ct) => step.Handle(input, branches, @default, ct), step.BuildBranchesPipelines, step.BuildDefaultPipeline);
    }

    /// <summary>
    /// Starts a pipeline with a single handler step.
    /// </summary>
    public EntryHandlerStep<TPipelineInput, TPipelineResult> StartWithHandler(string name, Handler<TPipelineInput, TPipelineResult> handler)
    {
        var step = new EntryHandlerStep<TPipelineInput, TPipelineResult>(name, handler, Builder);
        return step;
    }

    /// <summary>
    /// Starts a pipeline with a single handler step using provided step implementation.
    /// </summary>
    public EntryHandlerStep<TPipelineInput, TPipelineResult> StartWithHandler(IHandlerStep<TPipelineInput, TPipelineResult> step)
    {
        return StartWithHandler(step.Name, (input, ct) => step.Handle(input, ct));
    }
}



