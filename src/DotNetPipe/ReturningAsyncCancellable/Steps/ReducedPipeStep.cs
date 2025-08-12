using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.ForkSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.HandlerSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.IfElseSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.IfSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.LinearSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.MultiForkSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.SwitchSteps;

namespace K1vs.DotNetPipe.ReturningAsyncCancellable.Steps;

/// <summary>
/// Represents a reduced pipe step in a pipeline.
/// A reduced pipe step is a step that can call the next step in the pipeline.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input data for the entry step of the pipeline.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result produced by the entry step of the pipeline.</typeparam>
/// <typeparam name="TNextInput">The type of the input data for the next step in the pipeline.</typeparam>
/// <typeparam name="TNextResult">The type of the result produced by the next step in the pipeline.</typeparam>
public abstract class ReducedPipeStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult> : Step
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReducedPipeStep{TEntryStepInput, TEntryStepResult, TNextInput, TNextResult}"/> class.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="builder">The pipeline builder that contains the pipeline this step belongs to.</param>
    protected ReducedPipeStep(string name, PipelineBuilder builder) : base(name, builder) { }

    /// <summary>
    /// Builds an open pipeline from this reduced pipe step.
    /// An open pipeline starts with an entry step and ends with this reduced pipe step.
    /// </summary>
    /// <returns>An open pipeline that can be used as a sub-pipeline in another pipeline.</returns>
    public OpenPipeline<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult> BuildOpenPipeline()
    {
        if (Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        var openPipeline = new OpenPipeline<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult>(Builder.Name, Builder.EntryStep, this);
        Builder.Space.AddPipeline(openPipeline);
        return openPipeline;
    }

    /// <summary>
    /// Creates a linear step that follows this reduced pipe step in the pipeline.
    /// </summary>
    /// <typeparam name="TNextStepNextInput">The type of the input for the step after this linear step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result for the step after this linear step.</typeparam>
    /// <param name="name">The name of the next step.</param>
    /// <param name="next">The pipe that represents the next step in the pipeline.</param>
    /// <returns>A linear step that processes the input and calls the next step.</returns>
    public LinearStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult> ThenLinear<TNextStepNextInput, TNextStepNextResult>(
        string name,
        Pipe<TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult> next)
    {
        return new PipeLinearStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult>(this, name, next, Builder);
    }

    /// <summary>
    /// Creates a linear step that follows this reduced pipe step using a provided step implementation.
    /// </summary>
    /// <typeparam name="TNextStepNextInput">The type of the input for the step after this linear step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result for the step after this linear step.</typeparam>
    /// <param name="step">The linear step that represents the next step in the pipeline.</param>
    /// <returns>A linear step that processes the input and calls the next step.</returns>
    public LinearStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult> ThenLinear<TNextStepNextInput, TNextStepNextResult>(
        ILinearStep<TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult> step)
    {
        return ThenLinear<TNextStepNextInput, TNextStepNextResult>(step.Name, (input, next, ct) => step.Handle(input, next, ct));
    }

    /// <summary>
    /// Creates an if step that follows this reduced pipe step in the pipeline.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input for the if sub-pipeline.</typeparam>
    /// <typeparam name="TIfResult">The type of the result for the if sub-pipeline.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input for the step after the if step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result for the step after the if step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that decides whether to call the true branch.</param>
    /// <param name="trueBuilder">The builder for the true branch sub-pipeline.</param>
    /// <returns>An if step that follows this reduced pipe step.</returns>
    public IfStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult> ThenIf<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>(
        string name,
        IfSelector<TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>> trueBuilder)
    {
        return new PipeIfStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>(this, name, selector, trueBuilder, Builder);
    }

    /// <summary>
    /// Creates an if step that follows this reduced pipe step using a provided step implementation.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input for the if sub-pipeline.</typeparam>
    /// <typeparam name="TIfResult">The type of the result for the if sub-pipeline.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input for the step after the if step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result for the step after the if step.</typeparam>
    /// <param name="step">The if step implementation.</param>
    /// <returns>An if step that follows this reduced pipe step.</returns>
    public IfStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult> ThenIf<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>(
        IIfStep<TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult> step)
    {
        return ThenIf<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>(step.Name, step.Handle, step.BuildTruePipeline);
    }

    /// <summary>
    /// Creates an if-else step that follows this reduced pipe step in the pipeline.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input for the true branch.</typeparam>
    /// <typeparam name="TIfResult">The type of the result for the true branch.</typeparam>
    /// <typeparam name="TElseInput">The type of the input for the false branch.</typeparam>
    /// <typeparam name="TElseResult">The type of the result for the false branch.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input for the step after the if-else step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result for the step after the if-else step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that decides which branch to execute.</param>
    /// <param name="trueBuilder">The builder for the true branch sub-pipeline.</param>
    /// <param name="falseBuilder">The builder for the false branch sub-pipeline.</param>
    /// <returns>An if-else step that follows this reduced pipe step.</returns>
    public IfElseStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult> ThenIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>(
        string name,
        IfElseSelector<TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>> falseBuilder)
    {
        return new PipeIfElseStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>(this, name, selector, trueBuilder, falseBuilder, Builder);
    }

    /// <summary>
    /// Creates an if-else step that follows this reduced pipe step using a provided step implementation.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input for the true branch.</typeparam>
    /// <typeparam name="TIfResult">The type of the result for the true branch.</typeparam>
    /// <typeparam name="TElseInput">The type of the input for the false branch.</typeparam>
    /// <typeparam name="TElseResult">The type of the result for the false branch.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input for the step after the if-else step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result for the step after the if-else step.</typeparam>
    /// <param name="step">The if-else step implementation.</param>
    /// <returns>An if-else step that follows this reduced pipe step.</returns>
    public IfElseStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult> ThenIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>(
        IIfElseStep<TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult> step)
    {
        return ThenIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>(step.Name, step.Handle, step.BuildTruePipeline, step.BuildFalsePipeline);
    }

    /// <summary>
    /// Creates a switch step that follows this reduced pipe step in the pipeline.
    /// </summary>
    /// <typeparam name="TCaseInput">The type of the input for the case branches.</typeparam>
    /// <typeparam name="TCaseResult">The type of the result for the case branches.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input for the default branch.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result for the default branch.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input for the step after the switch step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result for the step after the switch step.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The switch selector.</param>
    /// <param name="caseBuilder">A function that builds the case branches.</param>
    /// <param name="defaultBuilder">The default branch pipeline.</param>
    /// <returns>A switch step that follows this reduced pipe step.</returns>
    public SwitchStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult> ThenSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult>(
        string name,
        SwitchSelector<TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextStepNextInput, TNextStepNextResult>>> caseBuilder,
        OpenPipeline<TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult> defaultBuilder)
    {
        return new PipeSwitchStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult>(this, name, selector, caseBuilder, defaultBuilder, Builder);
    }

    /// <summary>
    /// Creates a switch step that follows this reduced pipe step using a provided step implementation.
    /// </summary>
    /// <param name="step">The switch step implementation.</param>
    /// <returns>A switch step that follows this reduced pipe step.</returns>
    public SwitchStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult> ThenSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult>(
        ISwitchStep<TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult> step)
    {
        return ThenSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult>(step.Name, (input, cases, def, ct) => step.Handle(input, cases, def, ct), step.BuildCasesPipelines, step.BuildDefaultPipeline(Builder.Space));
    }

    /// <summary>
    /// Creates a fork step that follows this reduced pipe step in the pipeline.
    /// </summary>
    /// <typeparam name="TBranchAInput">The type of the input for branch A.</typeparam>
    /// <typeparam name="TBranchAResult">The type of the result for branch A.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the input for branch B.</typeparam>
    /// <typeparam name="TBranchBResult">The type of the result for branch B.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that decides which branch to execute.</param>
    /// <param name="branchABuilder">A function that builds the branch A pipeline.</param>
    /// <param name="branchBBuilder">A function that builds the branch B pipeline.</param>
    /// <returns>A fork step that follows this reduced pipe step.</returns>
    public ForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> ThenFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(
        string name,
        ForkSelector<TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> selector,
        Func<Space, Pipeline<TBranchAInput, TBranchAResult>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput, TBranchBResult>> branchBBuilder)
    {
        return new PipeForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(this, name, selector, branchABuilder, branchBBuilder, Builder);
    }

    /// <summary>
    /// Creates a fork step that follows this reduced pipe step using a provided step implementation.
    /// </summary>
    /// <param name="step">The fork step implementation.</param>
    /// <returns>A fork step that follows this reduced pipe step.</returns>
    public ForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> ThenFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(
        IForkStep<TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> step)
    {
        return ThenFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(step.Name, (input, a, b, ct) => step.Handle(input, a, b, ct), step.BuildBranchAPipeline, step.BuildBranchBPipeline);
    }

    /// <summary>
    /// Creates a multi-fork step that follows this reduced pipe step in the pipeline.
    /// </summary>
    /// <typeparam name="TBranchesInput">The type of the input for the branches.</typeparam>
    /// <typeparam name="TBranchesResult">The type of the result for the branches.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input for the default branch.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result for the default branch.</typeparam>
    /// <param name="name">The name of the step.</param>
    /// <param name="selector">The selector that decides which branch to execute.</param>
    /// <param name="branchesBuilder">A function that builds the branch pipelines.</param>
    /// <param name="defaultBuilder">A function that builds the default branch pipeline.</param>
    /// <returns>A multi-fork step that follows this reduced pipe step.</returns>
    public MultiForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> ThenMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(
        string name,
        MultiForkSelector<TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBuilder)
    {
        return new PipeMultiForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(this, name, selector, branchesBuilder, defaultBuilder, Builder);
    }

    /// <summary>
    /// Creates a multi-fork step that follows this reduced pipe step using a provided step implementation.
    /// </summary>
    /// <param name="step">The multi-fork step implementation.</param>
    /// <returns>A multi-fork step that follows this reduced pipe step.</returns>
    public MultiForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> ThenMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(
        IMultiForkStep<TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> step)
    {
        return ThenMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(step.Name, (input, branches, def, ct) => step.Handle(input, branches, def, ct), step.BuildBranchesPipelines, step.BuildDefaultPipeline);
    }

    /// <summary>
    /// Creates a handler step that follows this reduced pipe step in the pipeline.
    /// </summary>
    /// <param name="name">The name of the handler step.</param>
    /// <param name="delegate">The delegate that handles the input data.</param>
    /// <returns>A handler step that processes the input using the provided delegate.</returns>
    public HandlerStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult> HandleWith(string name, Handler<TNextInput, TNextResult> @delegate)
    {
        return new PipeHandlerStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult>(this, name, @delegate, Builder);
    }

    /// <summary>
    /// Creates a handler step that follows this reduced pipe step using a provided handler step implementation.
    /// </summary>
    /// <param name="step">The handler step.</param>
    /// <returns>A handler step that processes the input using the provided handler step.</returns>
    public HandlerStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult> HandleWith(IHandlerStep<TNextInput, TNextResult> step)
    {
        return HandleWith(step.Name, (input, ct) => step.Handle(input, ct));
    }

    /// <summary>
    /// Builds a handler for this reduced pipe step using the provided handler for the next step.
    /// </summary>
    /// <param name="handler">The handler for the next step.</param>
    /// <returns>The handler for the entry step of the pipeline.</returns>
    internal abstract Handler<TEntryStepInput, TEntryStepResult> BuildHandler(Handler<TNextInput, TNextResult> handler);
}



