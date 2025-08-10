using K1vs.DotNetPipe.Returning.Steps.ForkSteps;
using K1vs.DotNetPipe.Returning.Steps.HandlerSteps;
using K1vs.DotNetPipe.Returning.Steps.IfElseSteps;
using K1vs.DotNetPipe.Returning.Steps.IfSteps;
using K1vs.DotNetPipe.Returning.Steps.LinearSteps;
using K1vs.DotNetPipe.Returning.Steps.MultiForkSteps;
using K1vs.DotNetPipe.Returning.Steps.SwitchSteps;

namespace K1vs.DotNetPipe.Returning.Steps;

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
    /// <param name="builder">The builder that created this step, which contains the pipeline it belongs to.</param>
    protected ReducedPipeStep(string name, PipelineBuilder builder)
        : base(name, builder)
    {
    }

    /// <summary>
    /// Builds an open pipeline from this reduced pipe step.
    /// An open pipeline starts with an entry step and ends with this reduced pipe step.
    /// Open pipelines can be used as sub-pipelines in another pipeline, such as in if, ifelse, or switch steps.
    /// </summary>
    /// <returns>An open pipeline that can be used as a sub-pipeline in another pipeline.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the entry step is not set in the builder.</exception>
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
    /// A linear step is a step that processes the input data and calls the next step in the pipeline.
    /// The next step can be another linear step, an if step, a switch step, or a fork step.
    /// </summary>
    /// <typeparam name="TNextStepNextInput">The type of the input data for the next step after this linear step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result produced by the next step after this linear step.</typeparam>
    /// <param name="name">The name of the next step.</param>
    /// <param name="next">The pipe that represents the next step in the pipeline.</param>
    /// <returns>A linear step that processes the input data and calls the next step in the pipeline.</returns>
    public LinearStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult> ThenLinear<TNextStepNextInput, TNextStepNextResult>(string name,
        Pipe<TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult> next)
    {
        return new PipeLinearStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult>(this, name, next, Builder);
    }

    /// <summary>
    /// Creates a linear step that follows this reduced pipe step in the pipeline.
    /// A linear step is a step that processes the input data and calls the next step in the pipeline.
    /// The next step can be another linear step, an if step, a switch step, or a fork step.
    /// </summary>
    /// <typeparam name="TNextStepNextInput">The type of the input data for the next step after this linear step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result produced by the next step after this linear step.</typeparam>
    /// <param name="step">The linear step that represents the next step in the pipeline.</param>
    /// <returns>A linear step that processes the input data and calls the next step in the pipeline.</returns>
    public LinearStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult> ThenLinear<TNextStepNextInput, TNextStepNextResult>(ILinearStep<TNextInput, TNextResult, TNextStepNextInput, TNextStepNextResult> step)
    {
        return ThenLinear<TNextStepNextInput, TNextStepNextResult>(step.Name, step.Handle);
    }

    /// <summary>
    /// Creates an if step that follows this reduced pipe step in the pipeline.
    /// An if step is a conditional step that processes the input data and calls a call true branch if the condition is met,
    /// or skips to the next step if the condition is not met.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if step.</typeparam>
    /// <typeparam name="TIfResult">The type of the result from the if step.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input data for the next step after the if step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result from the next step after the if step.</typeparam>
    /// <param name="name">The name of the if step.</param>
    /// <param name="selector">The selector that determines if the if step should be taken.</param>
    /// <param name="trueBuilder">The builder for the true branch of the if step.</param>
    /// <returns>An if step that follows this reduced pipe step in the pipeline.</returns>
    public IfStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult> ThenIf<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>(string name,
        IfSelector<TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>> trueBuilder)
    {
        return new PipeIfStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>(this, name, selector, trueBuilder, Builder);
    }

    /// <summary>
    /// Creates an if step that follows this reduced pipe step in the pipeline.
    /// An if step is a conditional step that processes the input data and calls a call true branch if the condition is met,
    /// or skips to the next step if the condition is not met.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if step.</typeparam>
    /// <typeparam name="TIfResult">The type of the result from the if step.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input data for the next step after the if step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result from the next step after the if step.</typeparam>
    /// <param name="step">The if step that represents the conditional logic for the pipeline.</param>
    /// <returns>An if step that follows this reduced pipe step in the pipeline.</returns>
    public IfStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult> ThenIf<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>(IIfStep<TNextInput, TNextResult, TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult> step)
    {
        return ThenIf<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>(step.Name, step.Handle, step.BuildTruePipeline);
    }

    /// <summary>
    /// Creates an if-else step that follows this reduced pipe step in the pipeline.
    /// An if-else step is a conditional step that processes the input data and calls the true branch if the condition is met,
    /// or calls the false branch if the condition is not met.
    /// After the if-else step, the next step in the pipeline is called.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if step.</typeparam>
    /// <typeparam name="TIfResult">The type of the result from the if step.</typeparam>
    /// <typeparam name="TElseInput">The type of the input data for the else step.</typeparam>
    /// <typeparam name="TElseResult">The type of the result from the else step.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input data for the next step after the if-else step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result from the next step after the if-else step.</typeparam>
    /// <param name="name">The name of the if-else step.</param>
    /// <param name="selector">The selector that determines if the true or false branch should be taken.</param>
    /// <param name="trueBuilder">The builder for the true branch of the if-else step.</param>
    /// <param name="falseBuilder">The builder for the false branch of the if-else step.</param>
    /// <returns>An if-else step that follows this reduced pipe step in the pipeline.</returns>
    public IfElseStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult> ThenIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>(string name,
        IfElseSelector<TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult> selector,
        Func<Space, OpenPipeline<TIfInput, TIfResult, TNextStepNextInput, TNextStepNextResult>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>> falseBuilder)
    {
        return new PipeIfElseStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>(this, name, selector, trueBuilder, falseBuilder, Builder);
    }

    /// <summary>
    /// Creates an if-else step that follows this reduced pipe step in the pipeline.
    /// An if-else step is a conditional step that processes the input data and calls the true branch if the condition is met,
    /// or calls the false branch if the condition is not met.
    /// After the if-else step, the next step in the pipeline is called.
    /// </summary>
    /// <typeparam name="TIfInput">The type of the input data for the if step.</typeparam>
    /// <typeparam name="TIfResult">The type of the result from the if step.</typeparam>
    /// <typeparam name="TElseInput">The type of the input data for the else step.</typeparam>
    /// <typeparam name="TElseResult">The type of the result from the else step.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input data for the next step after the if-else step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result from the next step after the if-else step.</typeparam>
    /// <param name="step">The if-else step that represents the conditional logic for the pipeline.</param>
    /// <returns>An if-else step that follows this reduced pipe step in the pipeline.</returns>
    public IfElseStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult> ThenIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>(
        IIfElseStep<TNextInput, TNextResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult> step)
    {
        return ThenIfElse<TIfInput, TIfResult, TElseInput, TElseResult, TNextStepNextInput, TNextStepNextResult>(step.Name, step.Handle, step.BuildTruePipeline, step.BuildFalsePipeline);
    }

    /// <summary>
    /// Creates a switch step that follows this reduced pipe step in the pipeline.
    /// A switch step is a conditional step that processes the input data and calls one of the case branches based on the selector.
    /// If no case matches, the default branch is called.
    /// After the switch step, the next step in the pipeline is called.
    /// </summary>
    /// <typeparam name="TCaseInput">The type of the input data for the case branches.</typeparam>
    /// <typeparam name="TCaseResult">The type of the result from the case branches.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default branch.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result from the default branch.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input data for the next step after the switch step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result from the next step after the switch step.</typeparam>
    /// <param name="name">The name of the switch step.</param>
    /// <param name="selector">The selector that determines which case branch to take.</param>
    /// <param name="caseBuilder">A function that builds the case branches based on the input data.</param>
    /// <param name="defaultBuilder">The builder for the default branch of the switch step.</param>
    /// <returns>A switch step that follows this reduced pipe step in the pipeline.</returns>
    public SwitchStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult> ThenSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult>(string name,
        SwitchSelector<TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextStepNextInput, TNextStepNextResult>>> caseBuilder,
        OpenPipeline<TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult> defaultBuilder)
    {
        return new PipeSwitchStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult>(this, name, selector, caseBuilder, defaultBuilder, Builder);
    }

    /// <summary>
    /// Creates a switch step that follows this reduced pipe step in the pipeline.
    /// A switch step is a conditional step that processes the input data and calls one of the case branches based on the selector.
    /// If no case matches, the default branch is called.
    /// After the switch step, the next step in the pipeline is called.
    /// </summary>
    /// <typeparam name="TCaseInput">The type of the input data for the case branches.</typeparam>
    /// <typeparam name="TCaseResult">The type of the result from the case branches.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default branch.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result from the default branch.</typeparam>
    /// <typeparam name="TNextStepNextInput">The type of the input data for the next step after the switch step.</typeparam>
    /// <typeparam name="TNextStepNextResult">The type of the result from the next step after the switch step.</typeparam>
    /// <param name="step">The switch step that represents the conditional logic for the pipeline.</param>
    /// <returns>A switch step that follows this reduced pipe step in the pipeline.</returns>
    public SwitchStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult> ThenSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult>(
        ISwitchStep<TNextInput, TNextResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult> step)
    {
        return ThenSwitch<TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepNextInput, TNextStepNextResult>(step.Name, step.Handle, step.BuildCasesPipelines, step.BuildDefaultPipeline(Builder.Space));
    }

    /// <summary>
    /// Creates a fork step that follows this reduced pipe step in the pipeline.
    /// A fork step is a step that processes the input data and splits it into two branches,
    /// each of which can have its own pipeline.
    /// </summary>
    /// <typeparam name="TBranchAInput">The type of the input data for branch A.</typeparam>
    /// <typeparam name="TBranchAResult">The type of the result from branch A.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the input data for branch B.</typeparam>
    /// <typeparam name="TBranchBResult">The type of the result from branch B.</typeparam>
    /// <param name="name">The name of the fork step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchABuilder">A function that builds the pipeline for branch A.</param>
    /// <param name="branchBBuilder">A function that builds the pipeline for branch B.</param>
    /// <returns>A fork step that follows this reduced pipe step in the pipeline.</returns>
    public ForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> ThenFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(string name,
        ForkSelector<TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> selector,
        Func<Space, Pipeline<TBranchAInput, TBranchAResult>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput, TBranchBResult>> branchBBuilder)
    {
        return new PipeForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(this, name, selector, branchABuilder, branchBBuilder, Builder);
    }

    /// <summary>
    /// Creates a fork step that follows this reduced pipe step in the pipeline.
    /// A fork step is a step that processes the input data and splits it into two branches,
    /// each of which can have its own pipeline.
    /// </summary>
    /// <typeparam name="TBranchAInput">The type of the input data for branch A.</typeparam>
    /// <typeparam name="TBranchAResult">The type of the result from branch A.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the input data for branch B.</typeparam>
    /// <typeparam name="TBranchBResult">The type of the result from branch B.</typeparam>
    /// <param name="step">The fork step that represents the branching logic for the pipeline.</param>
    /// <returns>A fork step that follows this reduced pipe step in the pipeline.</returns>
    public ForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> ThenFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(IForkStep<TNextInput, TNextResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> step)
    {
        return ThenFork<TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(step.Name, step.Handle, step.BuildBranchAPipeline, step.BuildBranchBPipeline);
    }

    /// <summary>
    /// Creates a multi-fork step that follows this reduced pipe step in the pipeline.
    /// A multi-fork step is a step that processes the input data and splits it into multiple branches,
    /// each of which can have its own pipeline, and a default branch that is executed if no branch matches the input data.
    /// </summary>
    /// <typeparam name="TBranchesInput">The type of the input data for the branches.</typeparam>
    /// <typeparam name="TBranchesResult">The type of the result from the branches.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default branch.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result from the default branch.</typeparam>
    /// <param name="name">The name of the multi-fork step.</param>
    /// <param name="selector">The selector that determines which branch to take.</param>
    /// <param name="branchesBuilder">A function that builds the pipelines for the branches.</param>
    /// <param name="defaultBuilder">A function that builds the pipeline for the default branch.</param>
    /// <returns>A multi-fork step that follows this reduced pipe step in the pipeline.</returns>
    public MultiForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> ThenMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(string name,
        MultiForkSelector<TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBuilder)
    {
        return new PipeMultiForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(this, name, selector, branchesBuilder, defaultBuilder, Builder);
    }

    /// <summary>
    /// Creates a multi-fork step that follows this reduced pipe step in the pipeline.
    /// A multi-fork step is a step that processes the input data and splits it into multiple branches,
    /// each of which can have its own pipeline, and a default branch that is executed if no branch matches the input data.
    /// </summary>
    /// <typeparam name="TBranchesInput">The type of the input data for the branches.</typeparam>
    /// <typeparam name="TBranchesResult">The type of the result from the branches.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the input data for the default branch.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the result from the default branch.</typeparam>
    /// <param name="step">The multi-fork step that represents the branching logic for the pipeline.</param>
    /// <returns>A multi-fork step that follows this reduced pipe step in the pipeline.</returns>
    public MultiForkStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> ThenMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(IMultiForkStep<TNextInput, TNextResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> step)
    {
        return ThenMultiFork<TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(step.Name, step.Handle, step.BuildBranchesPipelines, step.BuildDefaultPipeline);
    }

    /// <summary>
    /// Creates a handler step that follows this reduced pipe step in the pipeline.
    /// A handler step is a step that processes the input data using a delegate to handle the data.
    /// </summary>
    /// <param name="name">The name of the handler step.</param>
    /// <param name="delegate">The delegate that handles the input data.</param>
    /// <returns>A pipe handler step that processes the input data using the provided delegate.</returns>
    public HandlerStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult> HandleWith(string name, Handler<TNextInput, TNextResult> @delegate)
    {
        return new PipeHandlerStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult>(this, name, @delegate, Builder);
    }

    /// <summary>
    /// Creates a handler step that follows this reduced pipe step in the pipeline.
    /// A handler step is a step that processes the input data using a delegate to handle the data.
    /// </summary>
    /// <param name="step">The handler step that processes the input data.</param>
    /// <returns>A pipe handler step that processes the input data using the provided handler step.</returns>
    public HandlerStep<TEntryStepInput, TEntryStepResult, TNextInput, TNextResult> HandleWith(IHandlerStep<TNextInput, TNextResult> step)
    {
        return HandleWith(step.Name, step.Handle);
    }

    /// <summary>
    /// Builds a handler for this reduced pipe step using the provided handler for the next step.
    /// </summary>
    /// <param name="handler">The handler for the next step.</param>
    internal abstract Handler<TEntryStepInput, TEntryStepResult> BuildHandler(Handler<TNextInput, TNextResult> handler);
}
