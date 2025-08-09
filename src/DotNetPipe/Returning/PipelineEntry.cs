using K1vs.DotNetPipe.Returning.Steps.ForkSteps;
using K1vs.DotNetPipe.Returning.Steps.HandlerSteps;
using K1vs.DotNetPipe.Returning.Steps.IfElseSteps;
using K1vs.DotNetPipe.Returning.Steps.IfSteps;
using K1vs.DotNetPipe.Returning.Steps.LinearSteps;
using K1vs.DotNetPipe.Returning.Steps.MultiForkSteps;
using K1vs.DotNetPipe.Returning.Steps.SwitchSteps;

namespace K1vs.DotNetPipe.Returning;

/// <summary>
/// PipelineEntry is the entry point for building a pipeline.
/// It allows you to define the first step of the pipeline.
/// </summary>
/// <typeparam name="TPipelineInput">The type of the input data for the pipeline.</typeparam>
/// <typeparam name="TPipelineResult">The type of the result produced by the pipeline.</typeparam>
public class PipelineEntry<TPipelineInput, TPipelineResult>
{
    /// <summary>
    /// Builder is the pipeline builder that allows you to define the steps of the pipeline.
    /// </summary>
    public PipelineBuilder Builder { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineEntry{TPipelineInput, TPipelineResult}"/> class.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    internal PipelineEntry(PipelineBuilder builder)
    {
        Builder = builder;
    }

    /// <summary>
    /// Starts a new linear step in the pipeline with the specified name and next step.
    /// This method allows you to define the first step of the pipeline, which will process the input data
    /// and pass the result to the next step.
    /// </summary>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <typeparam name="TNextResult">The type of the result produced by the next step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="next">The next step in the pipeline.</param>
    /// <returns>An instance of <see cref="EntryLinearStep{TPipelineInput, TPipelineResult, TNextInput, TNextResult}"/> representing the linear step.</returns>
    public EntryLinearStep<TPipelineInput, TPipelineResult, TNextInput, TNextResult> StartWithLinear<TNextInput, TNextResult>(string name,
        Pipe<TPipelineInput, TPipelineResult, TNextInput, TNextResult> next)
    {
        var step = new EntryLinearStep<TPipelineInput, TPipelineResult, TNextInput, TNextResult>(name, next, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new linear step in the pipeline with the specified name and next step.
    /// This method allows you to define the first step of the pipeline, which will process the input data
    /// and pass the result to the next step.
    /// </summary>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <typeparam name="TNextResult">The type of the result from the next step.</typeparam>
    /// <param name="step">The linear step that will handle the input data and produce the output data.</param>
    /// <returns>An instance of <see cref="EntryLinearStep{TPipelineInput, TPipelineResult, TNextInput, TNextResult}"/> representing the linear step.</returns>
    public EntryLinearStep<TPipelineInput, TPipelineResult, TNextInput, TNextResult> StartWithLinear<TNextInput, TNextResult>(ILinearStep<TPipelineInput, TPipelineResult, TNextInput, TNextResult> step)
    {
        return StartWithLinear<TNextInput, TNextResult>(step.Name, step.Handle);
    }

    /// <summary>
    /// Starts a new if step in the pipeline with the specified name and selector.
    /// This method allows you to define a conditional step in the pipeline, which can call the true sub-pipeline if required.
    /// Selector can call true pipeline if required, otherwise it will skip the step and continue with the next step in the pipeline.
    /// If true pipeline is called, it should call next step in the pipeline or stop execution.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if sub-pipeline.</typeparam>
    /// <typeparam name="TIfResult">The type of the result from the if sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <typeparam name="TNextResult">The type of the result from the next step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the if step.</param>
    /// <param name="trueBuilder">The builder for the true sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntryIfStep{TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult}"/> representing the if step.</returns>
    public EntryIfStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult> StartWithIf<TIfInput, TIfResult, TNextInput, TNextResult>(string name,
        IfSelector<TPipelineInput, TIfInput, TNextInput, TPipelineResult, TIfResult, TNextResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextInput, TNextResult>> trueBuilder)
    {
        var step = new EntryIfStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult>(name, selector, trueBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new if step in the pipeline with the specified name and selector.
    /// This method allows you to define a conditional step in the pipeline, which can call the true sub-pipeline if required.
    /// Selector can call true pipeline if required, otherwise it will skip the step and continue with the next step in the pipeline.
    /// If true pipeline is called, it should call next step in the pipeline or stop execution.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if sub-pipeline.</typeparam>
    /// <typeparam name="TIfResult">The type of the result from the if sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <typeparam name="TNextResult">The type of the result from the next step.</typeparam>
    /// <param name="step">The if step that will handle the input data and produce the output data.</param>
    /// <returns>An instance of <see cref="EntryIfStep{TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult}"/> representing the if step.</returns>
    public EntryIfStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult> StartWithIf<TIfInput, TIfResult, TNextInput, TNextResult>(IIfStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TNextInput, TNextResult> step)
    {
        return StartWithIf<TIfInput, TIfResult, TNextInput, TNextResult>(step.Name, step.Handle, step.BuildTruePipeline);
    }

    /// <summary>
    /// Starts a new if-else step in the pipeline with the specified name and selector.
    /// This method allows you to define a conditional step in the pipeline, which can call either the true or false sub-pipeline based on the selector.
    /// Selector can call true or false pipeline.
    /// True or false sub-pipeline should call next step in the pipeline or stop execution.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if sub-pipeline.</typeparam>
    /// <typeparam name="TIfResult">The type of the result from the if sub-pipeline.</typeparam>
    /// <typeparam name="TElseInput">The type of the input data for the else sub-pipeline.</typeparam>
    /// <typeparam name="TElseResult">The type of the result from the else sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <typeparam name="TNextResult">The type of the result from the next step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the if-else step.</param>
    /// <param name="trueBuilder">The builder for the true sub-pipeline.</param>
    /// <param name="falseBuilder">The builder for the false sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntryIfElseStep{TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult}"/> representing the if-else step.</returns>
    public EntryIfElseStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult> StartWithIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult>(string name,
        IfElseSelector<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextInput, TNextResult>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TElseResult, TNextInput, TNextResult>> falseBuilder)
    {
        var step = new EntryIfElseStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult>(name, selector, trueBuilder, falseBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new if-else step in the pipeline with the specified name and selector.
    /// This method allows you to define a conditional step in the pipeline, which can call either the true or false sub-pipeline based on the selector.
    /// Selector can call true or false pipeline.
    /// True or false sub-pipeline should call next step in the pipeline or stop execution.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if sub-pipeline.</typeparam>
    /// <typeparam name="TIfResult">The type of the result from the if sub-pipeline.</typeparam>
    /// <typeparam name="TElseInput">The type of the input data for the else sub-pipeline.</typeparam>
    /// <typeparam name="TElseResult">The type of the result from the else sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <typeparam name="TNextResult">The type of the result from the next step.</typeparam>
    /// <param name="step">The if-else step that will handle the input data and conditionally execute true or false branch.</param>
    /// <returns>An instance of <see cref="EntryIfElseStep{TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult}"/> representing the if-else step.</returns>
    public EntryIfElseStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult> StartWithIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult>(IIfElseStep<TPipelineInput, TPipelineResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult> step)
    {
        return StartWithIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult>(step.Name, step.Handle, step.BuildTruePipeline, step.BuildFalsePipeline);
    }

    /// <summary>
    /// Starts a new switch step in the pipeline with the specified name and selector.
    /// This method allows you to define a switch step in the pipeline, which can call one or more case sub-pipelines based on the selector.
    /// Selector can call one of the cases or default pipeline.
    /// Each case sub-pipeline should call next step in the pipeline or stop execution.
    /// If default pipeline is called, it should call next step in the pipeline or stop execution
    /// </summary>
    /// <typeparam name="TCaseInput">The type of the input data for the case sub-pipeline.</typeparam>
    /// <typeparam name="TCaseResult">The type of the result from the case sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result from the default sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <typeparam name="TNextResult">The type of the result from the next step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the switch step.</param>
    /// <param name="caseBuilder">A function that builds the case sub-pipelines.</param>
    /// <param name="defaultBuilder">The builder for the default sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntrySwitchStep{TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult}"/> representing the switch step.</returns>
    public EntrySwitchStep<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult> StartWithSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult>(string name,
        SwitchSelector<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextInput, TNextResult>>> caseBuilder,
        OpenPipeline<TDefaultInput, TDefaultResult, TNextInput, TNextResult> defaultBuilder)
    {
        var step = new EntrySwitchStep<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult>(name, selector, caseBuilder, defaultBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new switch step in the pipeline with the specified name and selector.
    /// This method allows you to define a switch step in the pipeline, which can call one or more case sub-pipelines based on the selector.
    /// Selector can call one of the cases or default pipeline.
    /// Each case sub-pipeline should call next step in the pipeline or stop execution.
    /// If default pipeline is called, it should call next step in the pipeline or stop execution
    /// </summary>
    /// <typeparam name="TCaseInput">The type of the input data for the case sub-pipeline.</typeparam>
    /// <typeparam name="TCaseResult">The type of the result from the case sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result from the default sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <typeparam name="TNextResult">The type of the result from the next step.</typeparam>
    /// <param name="step">The switch step that will handle the input data and route to appropriate case or default.</param>
    /// <returns>An instance of <see cref="EntrySwitchStep{TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult}"/> representing the switch step.</returns>
    public EntrySwitchStep<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult> StartWithSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult>(
        ISwitchStep<TPipelineInput, TPipelineResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult> step)
    {
        return StartWithSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult>(step.Name, step.Handle, step.BuildCasesPipelines, step.BuildDefaultPipeline(Builder.Space));
    }

    /// <summary>
    /// Starts a new fork step in the pipeline with the specified name and selector.
    /// This method allows you to define a fork step in the pipeline, which can call two branch sub-pipelines based on the selector.
    /// Selector can call branch A or branch B pipeline.
    /// Each branch sub-pipeline or default sub-pipeline should has its own handler at their end.
    /// </summary>
    /// <typeparam name="TBranchAInput">The type of the input data for branch A sub-pipeline.</typeparam>
    /// <typeparam name="TBranchAResult">The type of the result from branch A sub-pipeline.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the input data for branch B sub-pipeline.</typeparam>
    /// <typeparam name="TBranchBResult">The type of the result from branch B sub-pipeline.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the fork step.</param>
    /// <param name="branchABuilder">A function that builds the branch A sub-pipeline.</param>
    /// <param name="branchBBuilder">A function that builds the branch B sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntryForkStep{TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult}"/> representing the fork step.</returns>
    public EntryForkStep<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> StartWithFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(string name,
        ForkSelector<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> selector,
        Func<Space, Pipeline<TBranchAInput, TBranchAResult>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput, TBranchBResult>> branchBBuilder)
    {
        var step = new EntryForkStep<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(name, selector, branchABuilder, branchBBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new fork step in the pipeline with the specified name and selector.
    /// This method allows you to define a fork step in the pipeline, which can call two branch sub-pipelines based on the selector.
    /// Selector can call branch A or branch B pipeline.
    /// Each branch sub-pipeline or default sub-pipeline should has its own handler at their end.
    /// </summary>
    /// <typeparam name="TBranchAInput">The type of the input data for branch A sub-pipeline.</typeparam>
    /// <typeparam name="TBranchAResult">The type of the result from branch A sub-pipeline.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the input data for branch B sub-pipeline.</typeparam>
    /// <typeparam name="TBranchBResult">The type of the result from branch B sub-pipeline.</typeparam>
    /// <param name="step">The fork step that will handle the input data and route to appropriate branch.</param>
    /// <returns>An instance of <see cref="EntryForkStep{TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult}"/> representing the fork step.</returns>
    public EntryForkStep<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> StartWithFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(IForkStep<TPipelineInput, TPipelineResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> step)
    {
        return StartWithFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(step.Name, step.Handle, step.BuildBranchAPipeline, step.BuildBranchBPipeline);
    }

    /// <summary>
    /// Starts a new multi-fork step in the pipeline with the specified name and selector.
    /// This method allows you to define a multi-fork step in the pipeline, which can call multiple branch sub-pipelines based on the selector.
    /// Selector can call one of the branches or default pipeline.
    /// Each branch sub-pipeline or default sub-pipeline should has its own handler at their end.
    /// </summary>
    /// <typeparam name="TBranchesInput">The type of the input data for the branches sub-pipeline.</typeparam>
    /// <typeparam name="TBranchesResult">The type of the result from the branches sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result from the default sub-pipeline.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the multi-fork step.</param>
    /// <param name="branchesBuilder">A function that builds the branches sub-pipelines.</param>
    /// <param name="defaultBuilder">The builder for the default sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntryMultiForkStep{TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult}"/> representing the multi-fork step.</returns>
    public EntryMultiForkStep<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> StartWithMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(string name,
        MultiForkSelector<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBuilder)
    {
        var step = new EntryMultiForkStep<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(name, selector, branchesBuilder, defaultBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new multi-fork step in the pipeline with the specified name and selector.
    /// This method allows you to define a multi-fork step in the pipeline, which can call multiple branch sub-pipelines based on the selector.
    /// Selector can call one of the branches or default pipeline.
    /// Each branch sub-pipeline or default sub-pipeline should has its own handler at their end.
    /// </summary>
    /// <typeparam name="TBranchesInput">The type of the input data for the branches sub-pipeline.</typeparam>
    /// <typeparam name="TBranchesResult">The type of the result from the branches sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result from the default sub-pipeline.</typeparam>
    /// <param name="step">The multi-fork step that will handle the input data and route to appropriate branch or default.</param>
    /// <returns>An instance of <see cref="EntryMultiForkStep{TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult}"/> representing the multi-fork step.</returns>
    public EntryMultiForkStep<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> StartWithMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(IMultiForkStep<TPipelineInput, TPipelineResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> step)
    {
        return StartWithMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(step.Name, step.Handle, step.BuildBranchesPipelines, step.BuildDefaultPipeline);
    }

    /// <summary>
    /// This method allows you to define a pipeline that consists of a single handler step.
    /// </summary>
    /// <param name="name">The name of the handler step.</param>
    /// <param name="handler">The handler that will process the input data.</param>
    /// <returns>An instance of <see cref="EntryHandlerStep{TPipelineInput, TPipelineResult}"/> representing the handler step.</returns>
    public EntryHandlerStep<TPipelineInput, TPipelineResult> StartWithHandler(string name, Handler<TPipelineInput, TPipelineResult> handler)
    {
        var step = new EntryHandlerStep<TPipelineInput, TPipelineResult>(name, handler, Builder);
        return step;
    }

    /// <summary>
    /// This method allows you to define a pipeline that consists of a single handler step.
    /// </summary>
    /// <param name="step">The handler step that will process the input data.</param>
    /// <returns>An instance of <see cref="EntryHandlerStep{TPipelineInput, TPipelineResult}"/> representing the handler step.</returns>
    public EntryHandlerStep<TPipelineInput, TPipelineResult> StartWithHandler(IHandlerStep<TPipelineInput, TPipelineResult> step)
    {
        return StartWithHandler(step.Name, step.Handle);
    }
}
