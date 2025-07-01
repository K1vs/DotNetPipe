using K1vs.DotNetPipe.AsyncCancellable.Steps.ForkSteps;
using K1vs.DotNetPipe.AsyncCancellable.Steps.HandlerSteps;
using K1vs.DotNetPipe.AsyncCancellable.Steps.IfElseSteps;
using K1vs.DotNetPipe.AsyncCancellable.Steps.IfSteps;
using K1vs.DotNetPipe.AsyncCancellable.Steps.LinearSteps;
using K1vs.DotNetPipe.AsyncCancellable.Steps.MultiForkSteps;
using K1vs.DotNetPipe.AsyncCancellable.Steps.SwitchSteps;

namespace K1vs.DotNetPipe.AsyncCancellable;

/// <summary>
/// PipelineEntry is the entry point for building a pipeline.
/// It allows you to define the first step of the pipeline.
/// </summary>
/// <typeparam name="TPipelineInput">The type of the input data for the pipeline.</typeparam>
public class PipelineEntry<TPipelineInput>
{
    /// <summary>
    /// Builder is the pipeline builder that allows you to define the steps of the pipeline.
    /// </summary>
    public PipelineBuilder Builder { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineEntry{TPipelineInput}"/> class.
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
    /// <param name="name">The name of the step.</param>
    /// <param name="next">The next step in the pipeline.</param>
    /// <returns>An instance of <see cref="EntryLinearStep{TPipelineInput, TNextInput}"/> representing the linear step.</returns>
    public EntryLinearStep<TPipelineInput, TNextInput> StartWithLinear<TNextInput>(string name, Pipe<TPipelineInput, TNextInput> next)
    {
        var step = new EntryLinearStep<TPipelineInput, TNextInput>(name, next, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new linear step in the pipeline with the specified name and next step.
    /// This method allows you to define the first step of the pipeline, which will process the input data
    /// and pass the result to the next step.
    /// </summary>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <param name="step">The linear step that will handle the input data and produce the output data.</param>
    /// <returns>An instance of <see cref="EntryLinearStep{TPipelineInput, TNextInput}"/> representing the linear step.</returns>
    public EntryLinearStep<TPipelineInput, TNextInput> StartWithLinear<TNextInput>(ILinearStep<TPipelineInput, TNextInput> step)
    {
        return StartWithLinear<TNextInput>(step.Name, step.Handle);
    }

    /// <summary>
    /// Starts a new if step in the pipeline with the specified name and selector.
    /// This method allows you to define a conditional step in the pipeline, which can call the true sub-pipeline if required.
    /// Selector can call true pipeline if required, otherwise it will skip the step and continue with the next step in the pipeline.
    /// If true pipeline is called, it should call next step in the pipeline or stop execution.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the if step.</param>
    /// <param name="trueBuilder">The builder for the true sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntryIfStep{TPipelineInput, TIfInput, TNextInput}"/> representing the if step.</returns>
    public EntryIfStep<TPipelineInput, TIfInput, TNextInput> StartWithIf<TIfInput, TNextInput>(string name,
        IfSelector<TPipelineInput, TIfInput, TNextInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextInput>> trueBuilder)
    {
        var step = new EntryIfStep<TPipelineInput, TIfInput, TNextInput>(name, selector, trueBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new if step in the pipeline with the specified name and selector.
    /// This method allows you to define a conditional step in the pipeline, which can call the true sub-pipeline if required.
    /// Selector can call true pipeline if required, otherwise it will skip the step and continue with the next step in the pipeline.
    /// If true pipeline is called, it should call next step in the pipeline or stop execution.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <param name="step">The if step that will handle the input data and produce the output data.</param>
    /// <returns>An instance of <see cref="EntryIfStep{TPipelineInput, TIfInput, TNextInput}"/> representing the if step.</returns>
    public EntryIfStep<TPipelineInput, TIfInput, TNextInput> StartWithIf<TIfInput, TNextInput>(IIfStep<TPipelineInput, TIfInput, TNextInput> step)
    {
        return StartWithIf(step.Name, step.Handle, step.BuildTruePipeline);
    }

    /// <summary>
    /// Starts a new if-else step in the pipeline with the specified name and selector.
    /// This method allows you to define a conditional step in the pipeline, which can call either the true or false sub-pipeline based on the selector.
    /// Selector can call true or false pipeline.
    /// True or false sub-pipeline should call next step in the pipeline or stop execution.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if sub-pipeline.</typeparam>
    /// <typeparam name="TElseInput">The type of the input data for the else sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the if-else step.</param>
    /// <param name="trueBuilder">The builder for the true sub-pipeline.</param>
    /// <param name="falseBuilder">The builder for the false sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntryIfElseStep{TPipelineInput, TIfInput, TElseInput, TNextInput}"/> representing the if-else step.</returns>
    public EntryIfElseStep<TPipelineInput, TIfInput, TElseInput, TNextInput> StartWithIfElse<TIfInput, TElseInput, TNextInput>(string name,
        IfElseSelector<TPipelineInput, TIfInput, TElseInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextInput>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TNextInput>> falseBuilder)
    {
        var step = new EntryIfElseStep<TPipelineInput, TIfInput, TElseInput, TNextInput>(name, selector, trueBuilder, falseBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new if-else step in the pipeline with the specified name and selector.
    /// This method allows you to define a conditional step in the pipeline, which can call either the true or false sub-pipeline based on the selector.
    /// Selector can call true or false pipeline.
    /// True or false sub-pipeline should call next step in the pipeline or stop execution.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if sub-pipeline.</typeparam>
    /// <typeparam name="TElseInput">The type of the input data for the else sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <param name="step">The if-else step that will handle the input data and conditionally execute true or false branch.</param>
    /// <returns>An instance of <see cref="EntryIfElseStep{TPipelineInput, TIfInput, TElseInput, TNextInput}"/> representing the if-else step.</returns>
    public EntryIfElseStep<TPipelineInput, TIfInput, TElseInput, TNextInput> StartWithIfElse<TIfInput, TElseInput, TNextInput>(IIfElseStep<TPipelineInput, TIfInput, TElseInput, TNextInput> step)
    {
        return StartWithIfElse(step.Name, step.Handle, step.BuildTruePipeline, step.BuildFalsePipeline);
    }

    /// <summary>
    /// Starts a new switch step in the pipeline with the specified name and selector.
    /// This method allows you to define a switch step in the pipeline, which can call one or more case sub-pipelines based on the selector.
    /// Selector can call one of the cases or default pipeline.
    /// Each case sub-pipeline should call next step in the pipeline or stop execution.
    /// If default pipeline is called, it should call next step in the pipeline or stop execution
    /// </summary>
    /// <typeparam name="TCaseInput">The type of the input data for the case sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the switch step.</param>
    /// <param name="caseBuilder">A function that builds the case sub-pipelines.</param>
    /// <param name="defaultBuilder">The builder for the default sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntrySwitchStep{TPipelineInput, TCaseInput, TDefaultInput, TNextInput}"/> representing the switch step.</returns>
    public EntrySwitchStep<TPipelineInput, TCaseInput, TDefaultInput, TNextInput> StartWithSwitch<TCaseInput, TDefaultInput, TNextInput>(string name,
        SwitchSelector<TPipelineInput, TCaseInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextInput>>> caseBuilder,
        OpenPipeline<TDefaultInput, TNextInput> defaultBuilder)
    {
        var step = new EntrySwitchStep<TPipelineInput, TCaseInput, TDefaultInput, TNextInput>(name, selector, caseBuilder, defaultBuilder, Builder);
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
    /// <typeparam name="TDefaultInput">The type of the input data for the default sub-pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the input data for the next step.</typeparam>
    /// <param name="step">The switch step that will handle the input data and route to appropriate case or default.</param>
    /// <returns>An instance of <see cref="EntrySwitchStep{TPipelineInput, TCaseInput, TDefaultInput, TNextInput}"/> representing the switch step.</returns>
    public EntrySwitchStep<TPipelineInput, TCaseInput, TDefaultInput, TNextInput> StartWithSwitch<TCaseInput, TDefaultInput, TNextInput>(
        ISwitchStep<TPipelineInput, TCaseInput, TDefaultInput, TNextInput> step)
    {
        return StartWithSwitch(step.Name, step.Handle, step.BuildCasesPipelines, step.BuildDefaultPipeline(Builder.Space));
    }

    /// <summary>
    /// Starts a new fork step in the pipeline with the specified name and selector.
    /// This method allows you to define a fork step in the pipeline, which can call two branch sub-pipelines based on the selector.
    /// Selector can call branch A or branch B pipeline.
    /// Each branch sub-pipeline or default sub-pipeline should has its own handler at their end.
    /// </summary>
    /// <typeparam name="TBranchAInput">The type of the input data for branch A sub-pipeline.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the input data for branch B sub-pipeline.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the fork step.</param>
    /// <param name="branchABuilder">A function that builds the branch A sub-pipeline.</param>
    /// <param name="branchBBuilder">A function that builds the branch B sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntryForkStep{TPipelineInput, TBranchAInput, TBranchBInput}"/> representing the fork step.</returns>
    public EntryForkStep<TPipelineInput, TBranchAInput, TBranchBInput> StartWithFork<TBranchAInput, TBranchBInput>(string name,
        ForkSelector<TPipelineInput, TBranchAInput, TBranchBInput> selector,
        Func<Space, Pipeline<TBranchAInput>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput>> branchBBuilder)
    {
        var step = new EntryForkStep<TPipelineInput, TBranchAInput, TBranchBInput>(name, selector, branchABuilder, branchBBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new fork step in the pipeline with the specified name and selector.
    /// This method allows you to define a fork step in the pipeline, which can call two branch sub-pipelines based on the selector.
    /// Selector can call branch A or branch B pipeline.
    /// Each branch sub-pipeline or default sub-pipeline should has its own handler at their end.
    /// </summary>
    /// <typeparam name="TBranchAInput">The type of the input data for branch A sub-pipeline.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the input data for branch B sub-pipeline.</typeparam>
    /// <param name="step">The fork step that will handle the input data and route to appropriate branch.</param>
    /// <returns>An instance of <see cref="EntryForkStep{TPipelineInput, TBranchAInput, TBranchBInput}"/> representing the fork step.</returns>
    public EntryForkStep<TPipelineInput, TBranchAInput, TBranchBInput> StartWithFork<TBranchAInput, TBranchBInput>(IForkStep<TPipelineInput, TBranchAInput, TBranchBInput> step)
    {
        return StartWithFork(step.Name, step.Handle, step.BuildBranchAPipeline, step.BuildBranchBPipeline);
    }

    /// <summary>
    /// Starts a new multi-fork step in the pipeline with the specified name and selector.
    /// This method allows you to define a multi-fork step in the pipeline, which can call multiple branch sub-pipelines based on the selector.
    /// Selector can call one of the branches or default pipeline.
    /// Each branch sub-pipeline or default sub-pipeline should has its own handler at their end.
    /// </summary>
    /// <typeparam name="TBranchesInput">The type of the input data for the branches sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default sub-pipeline.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector for the multi-fork step.</param>
    /// <param name="branchesBuilder">A function that builds the branches sub-pipelines.</param>
    /// <param name="defaultBuilder">The builder for the default sub-pipeline.</param>
    /// <returns>An instance of <see cref="EntryMultiForkStep{TPipelineInput, TBranchesInput, TDefaultInput}"/> representing the multi-fork step.</returns>
    public EntryMultiForkStep<TPipelineInput, TBranchesInput, TDefaultInput> StartWithMultiFork<TBranchesInput, TDefaultInput>(string name,
        MultiForkSelector<TPipelineInput, TBranchesInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput>> defaultBuilder)
    {
        var step = new EntryMultiForkStep<TPipelineInput, TBranchesInput, TDefaultInput>(name, selector, branchesBuilder, defaultBuilder, Builder);
        return step;
    }

    /// <summary>
    /// Starts a new multi-fork step in the pipeline with the specified name and selector.
    /// This method allows you to define a multi-fork step in the pipeline, which can call multiple branch sub-pipelines based on the selector.
    /// Selector can call one of the branches or default pipeline.
    /// Each branch sub-pipeline or default sub-pipeline should has its own handler at their end.
    /// </summary>
    /// <typeparam name="TBranchesInput">The type of the input data for the branches sub-pipeline.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default sub-pipeline.</typeparam>
    /// <param name="step">The multi-fork step that will handle the input data and route to appropriate branch or default.</param>
    /// <returns>An instance of <see cref="EntryMultiForkStep{TPipelineInput, TBranchesInput, TDefaultInput}"/> representing the multi-fork step.</returns>
    public EntryMultiForkStep<TPipelineInput, TBranchesInput, TDefaultInput> StartWithMultiFork<TBranchesInput, TDefaultInput>(IMultiForkStep<TPipelineInput, TBranchesInput, TDefaultInput> step)
    {
        return StartWithMultiFork(step.Name, step.Handle, step.BuildBranchesPipelines, step.BuildDefaultPipeline);
    }

    /// <summary>
    /// This method allows you to define a pipeline that consists of a single handler step.
    /// </summary>
    /// <param name="name">The name of the handler step.</param>
    /// <param name="handler">The handler that will process the input data.</param>
    /// <returns>An instance of <see cref="EntryHandlerStep{TPipelineInput}"/> representing the handler step.</returns>
    public EntryHandlerStep<TPipelineInput> StartWithHandler(string name, Handler<TPipelineInput> handler)
    {
        var step = new EntryHandlerStep<TPipelineInput>(name, handler, Builder);
        return step;
    }

    /// <summary>
    /// This method allows you to define a pipeline that consists of a single handler step.
    /// </summary>
    /// <param name="step">The handler step that will process the input data.</param>
    /// <returns>An instance of <see cref="EntryHandlerStep{TPipelineInput}"/> representing the handler step.</returns>
    public EntryHandlerStep<TPipelineInput> StartWithHandler(IHandlerStep<TPipelineInput> step)
    {
        return StartWithHandler(step.Name, step.Handle);
    }
}
