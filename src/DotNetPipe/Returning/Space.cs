using K1vs.DotNetPipe.Returning.Steps;
using K1vs.DotNetPipe.Returning.Steps.HandlerSteps;
using K1vs.DotNetPipe.Returning.Steps.IfElseSteps;
using K1vs.DotNetPipe.Returning.Steps.IfSteps;
using K1vs.DotNetPipe.Returning.Steps.LinearSteps;
using K1vs.DotNetPipe.Returning.Steps.ForkSteps;
using K1vs.DotNetPipe.Returning.Steps.MultiForkSteps;
using K1vs.DotNetPipe.Returning.Steps.SwitchSteps;
using System.Collections.Immutable;
using K1vs.DotNetPipe.Exceptions;

namespace K1vs.DotNetPipe.Returning;

/// <summary>
/// Space is a container for pipelines and steps.
/// It allows you to create and manage pipelines, and provides methods to retrieve pipelines and steps by their names.
/// Retrieved steps can be modified by mutators, which can change the way steps are executed or how data is processed.
/// Each space can contain multiple pipelines, and each pipeline can have multiple steps.
/// </summary>
public class Space
{
    private readonly Dictionary<StepName, Step> _steps = [];

    private readonly Dictionary<string, Pipeline> _pipelines = [];

    /// <summary>
    /// Gets a read-only collection of all steps in the space.
    /// Each step can be accessed by its name, which is a combination of the step name and the pipeline name.
    /// Steps can be of various types, including linear steps, handler steps, if steps, and more.
    /// </summary>
    public IReadOnlyDictionary<StepName, Step> Steps => _steps.AsReadOnly();

    /// <summary>
    /// Gets a read-only collection of all pipelines in the space.
    /// Each pipeline can be accessed by its name, and it can contain multiple steps.
    /// Pipelines can be of various types, including pipelines and open pipelines.
    /// </summary>
    public IReadOnlyDictionary<string, Pipeline> Pipelines => _pipelines.AsReadOnly();

    /// <summary>
    /// Stating point for creating a new pipeline.
    /// This method initializes a new pipeline with the specified name.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <typeparam name="TOutput">The type of the output data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>A new pipeline entry.</returns>
    public PipelineEntry<TInput, TOutput> CreatePipeline<TInput, TOutput>(string name)
    {
        var builder = new PipelineBuilder(this, name);
        return new PipelineEntry<TInput, TOutput>(builder);
    }

    /// <summary>
    /// Retrieves a pipeline by its name.
    /// If the pipeline does not exist, it returns null.
    /// </summary>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The pipeline if found; otherwise, null.</returns>
    public Pipeline? GetPipeline(string name)
    {
        return _pipelines.GetValueOrDefault(name);
    }

    /// <summary>
    /// Retrieves a pipeline by its name.
    /// If the pipeline does not exist, it throws a PipelineNotFoundException.
    /// </summary>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The pipeline if found; otherwise, null.</returns>
    /// <exception cref="PipelineNotFoundException"></exception>
    public Pipeline? GetRequiredPipeline(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline))
        {
            return pipeline;
        }
        throw new PipelineNotFoundException(name);
    }

    /// <summary>
    /// Retrieves a pipeline by its name and casts it to the specified type.
    /// If the pipeline does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <typeparam name="TOutput">The type of the output data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The pipeline if found and of the correct type; otherwise, null.</returns>
    public Pipeline<TInput, TOutput>? GetPipeline<TInput, TOutput>(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline) && pipeline is Pipeline<TInput, TOutput> closedPipeline)
        {
            return closedPipeline;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a pipeline by its name and casts it to the specified type.
    /// If the pipeline exist but has a different type, it throws an UnexpectedPipelineTypeException.
    /// If the pipeline does not exist, it throws a PipelineNotFoundException.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <typeparam name="TOutput">The type of the output data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The pipeline if found and of the correct type; otherwise, null.</returns>
    /// <exception cref="PipelineNotFoundException"></exception>
    /// <exception cref="UnexpectedPipelineTypeException"></exception>
    public Pipeline<TInput, TOutput> GetRequiredPipeline<TInput, TOutput>(string name)
    {
        if (!_pipelines.TryGetValue(name, out var pipeline))
        {
            throw new PipelineNotFoundException(name);
        }
        if (pipeline is not Pipeline<TInput, TOutput> typedPipeline)
        {
            throw new UnexpectedPipelineTypeException(name, pipeline.GetType(), typeof(Pipeline<TInput, TOutput>));
        }
        return typedPipeline;
    }

    /// <summary>
    /// Retrieves an open pipeline by its name and casts it to the specified type.
    /// If the pipeline does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <typeparam name="TInputResult">The type of the input result for the pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the next input data for the pipeline.</typeparam>
    /// <typeparam name="TNextInputResult">The type of the next input result for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The open pipeline if found and of the correct type; otherwise, null.</returns>
    public OpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult>? GetOpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult>(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline) && pipeline is OpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult> openPipeline)
        {
            return openPipeline;
        }
        return null;
    }

    /// <summary>
    /// Retrieves an open pipeline by its name and casts it to the specified type.
    /// If the pipeline exists but has a different type, it throws an UnexpectedPipelineTypeException.
    /// If the pipeline does not exist, it throws a PipelineNotFoundException.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <typeparam name="TInputResult">The type of the input result for the pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the next input data for the pipeline.</typeparam>
    /// <typeparam name="TNextInputResult">The type of the next input result for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The open pipeline if found and of the correct type; otherwise, null.</returns>
    /// <exception cref="PipelineNotFoundException"></exception>
    /// <exception cref="UnexpectedPipelineTypeException"></exception>
    public OpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult> GetRequiredOpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult>(string name)
    {
        if (!_pipelines.TryGetValue(name, out var pipeline))
        {
            throw new PipelineNotFoundException(name);
        }
        if (pipeline is not OpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult> openPipeline)
        {
            throw new UnexpectedPipelineTypeException(name, pipeline.GetType(), typeof(OpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult>));
        }
        return openPipeline;
    }

    /// <summary>
    /// Retrieves a step by its name within a specific pipeline.
    /// If the step does not exist, it returns null.
    /// </summary>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The step if found; otherwise, null.</returns>
    public Step? GetStep(string pipeline, string name)
    {
        return _steps.GetValueOrDefault(new StepName(name, pipeline));
    }

    /// <summary>
    /// Retrieves a step by its name within a specific pipeline.
    /// If the step does not exist, it throws a StepNotFoundException.
    /// </summary>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The step if found; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    public Step GetRequiredStep(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        return step;
    }

    /// <summary>
    /// Retrieves a linear step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TNextInput">The type of the next step input.</typeparam>
    /// <typeparam name="TNextResult">The type of the next step result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The linear step if found and of the correct type; otherwise, null.</returns>
    public LinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult>? GetLinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is LinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult> pipelineStep)
        {
            return pipelineStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a linear step by its name within a specific pipeline.
    /// If the step exists but has a different type, it throws an UnexpectedStepTypeException.
    /// If the step does not exist, it throws a StepNotFoundException.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TNextInput">The type of the next step input.</typeparam>
    /// <typeparam name="TNextResult">The type of the next step result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The linear step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public LinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult> GetRequiredLinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not LinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult> pipelineStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(LinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult>));
        }
        return pipelineStep;
    }

    /// <summary>
    /// Retrieves a handler step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The handler step if found and of the correct type; otherwise, null.</returns>
    public HandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult>? GetHandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is HandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult> pipelineTerminator)
        {
            return pipelineTerminator;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a handler step by its name within a specific pipeline.
    /// If the step exists but has a different type, it throws an UnexpectedStepTypeException
    /// If the step does not exist, it throws a StepNotFoundException.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The handler step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public HandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult> GetRequiredHandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not HandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult> pipelineTerminator)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(HandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult>));
        }
        return pipelineTerminator;
    }

    /// <summary>
    /// Retrieves an if step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TIfInput">The type of the if step input.</typeparam>
    /// <typeparam name="TIfResult">The type of the if step result.</typeparam>
    /// <typeparam name="TNextStepInput">The type of the next step input.</typeparam>
    /// <typeparam name="TNextStepResult">The type of the next step result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The if step if found and of the correct type; otherwise, null.</returns>
    public IfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult>? GetIfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is IfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult> pipelineIfStep)
        {
            return pipelineIfStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves an if step by its name within a specific pipeline.
    /// If the step exists but has a different type, it throws an UnexpectedStepTypeException.
    /// If the step does not exist, it throws a StepNotFoundException.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TIfInput">The type of the if step input.</typeparam>
    /// <typeparam name="TIfResult">The type of the if step result.</typeparam>
    /// <typeparam name="TNextStepInput">The type of the next step input.</typeparam>
    /// <typeparam name="TNextStepResult">The type of the next step result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The if step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public IfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult> GetRequiredIfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not IfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult> pipelineIfStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(IfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult>));
        }
        return pipelineIfStep;
    }

    /// <summary>
    /// Retrieves an if-else step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TIfInput">The type of the if step input.</typeparam>
    /// <typeparam name="TIfResult">The type of the if step result.</typeparam>
    /// <typeparam name="TElseInput">The type of the else step input.</typeparam>
    /// <typeparam name="TElseResult">The type of the else step result.</typeparam>
    /// <typeparam name="TNextStepInput">The type of the next step input.</typeparam>
    /// <typeparam name="TNextStepResult">The type of the next step result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The if-else step if found and of the correct type; otherwise, null.</returns>
    public IfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult>? GetIfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is IfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult> pipelineIfElseStep)
        {
            return pipelineIfElseStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves an if-else step by its name within a specific pipeline.
    /// If the step exists but has a different type, it throws an UnexpectedStepTypeException
    /// If the step does not exist, it throws a StepNotFoundException.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TIfInput">The type of the if step input.</typeparam>
    /// <typeparam name="TIfResult">The type of the if step result.</typeparam>
    /// <typeparam name="TElseInput">The type of the else step input.</typeparam>
    /// <typeparam name="TElseResult">The type of the else step result.</typeparam>
    /// <typeparam name="TNextStepInput">The type of the next step input.</typeparam>
    /// <typeparam name="TNextStepResult">The type of the next step result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The if-else step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public IfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult> GetRequiredIfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not IfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult> pipelineIfElseStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(IfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult>));
        }
        return pipelineIfElseStep;
    }

    /// <summary>
    /// Retrieves a fork step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TBranchAInput">The type of the branch A input.</typeparam>
    /// <typeparam name="TBranchAResult">The type of the branch A result.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the branch B input.</typeparam>
    /// <typeparam name="TBranchBResult">The type of the branch B result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The fork step if found and of the correct type; otherwise, null.</returns>
    public ForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>? GetForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is ForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> forkStep)
        {
            return forkStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a fork step by its name within a specific pipeline.
    /// If the step exists but has a different type, it throws an UnexpectedStepTypeException
    /// If the step does not exist, it throws a StepNotFoundException.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TBranchAInput">The type of the branch A input.</typeparam>
    /// <typeparam name="TBranchAResult">The type of the branch A result.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the branch B input.</typeparam>
    /// <typeparam name="TBranchBResult">The type of the branch B result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The fork step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public ForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> GetRequiredForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not ForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> forkStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(ForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>));
        }
        return forkStep;
    }

    /// <summary>
    /// Retrieves a multi-fork step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TBranchesInput">The type of the branches input.</typeparam>
    /// <typeparam name="TBranchesResult">The type of the branches result.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the default input.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the default result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The multi-fork step if found and of the correct type; otherwise, null.</returns>
    public MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>? GetMultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> multiForkStep)
        {
            return multiForkStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a multi-fork step by its name within a specific pipeline.
    /// If the step exists but has a different type, it throws an UnexpectedStepTypeException.
    /// If the step does not exist, it throws a StepNotFoundException.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TBranchesInput">The type of the branches input.</typeparam>
    /// <typeparam name="TBranchesResult">The type of the branches result.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the default input.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the default result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The multi-fork step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> GetRequiredMultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> multiForkStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>));
        }
        return multiForkStep;
    }

    /// <summary>
    /// Retrieves a switch step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TCaseInput">The type of the case input.</typeparam>
    /// <typeparam name="TCaseResult">The type of the case result.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the default input.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the default result.</typeparam>
    /// <typeparam name="TNextStepInput">The type of the next step input.</typeparam>
    /// <typeparam name="TNextStepResult">The type of the next step result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The switch step if found and of the correct type; otherwise, null.</returns>
    public SwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult>? GetSwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is SwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> switchStep)
        {
            return switchStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a switch step by its name within a specific pipeline.
    /// If the step exists but has a different type, it throws an UnexpectedStepTypeException.
    /// If the step does not exist, it throws a StepNotFoundException.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TEntryStepResult">The type of the entry step result.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TResult">The type of the step result.</typeparam>
    /// <typeparam name="TCaseInput">The type of the case input.</typeparam>
    /// <typeparam name="TCaseResult">The type of the case result.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the default input.</typeparam>
    /// <typeparam name="TDefaultResult">The type of the default result.</typeparam>
    /// <typeparam name="TNextStepInput">The type of the next step input.</typeparam>
    /// <typeparam name="TNextStepResult">The type of the next step result.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The switch step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public SwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> GetRequiredSwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not SwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> switchStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(SwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult>));
        }
        return switchStep;
    }

    /// <summary>
    /// Adds a pipeline to the space.
    /// If a pipeline with the same name already exists, it throws a PipelineWithNameAlreadyExistsException.
    /// </summary>
    /// <param name="pipeline">The pipeline to add.</param>
    /// <exception cref="PipelineWithNameAlreadyExistsException"></exception>
    internal void AddPipeline(Pipeline pipeline)
    {
        if (!_pipelines.TryAdd(pipeline.Name, pipeline))
        {
            throw new PipelineWithNameAlreadyExistsException(pipeline.Name);
        }
    }

    /// <summary>
    /// Adds a step to the space.
    /// If a step with the same name already exists, it throws a StepWithNameAlreadyExistsException.
    /// The step is identified by its name, which is a combination of the step name and the pipeline name.
    /// </summary>
    /// <param name="step"> The step to add.</param>
    /// <exception cref="StepWithNameAlreadyExistsException"></exception>
    internal void AddStep(Step step)
    {
        if (!_steps.TryAdd(step.Name, step))
        {
            throw new StepWithNameAlreadyExistsException(step.Name.ToString());
        }
    }
}
