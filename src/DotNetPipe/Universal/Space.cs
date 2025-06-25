using K1vs.DotNetPipe.Universal.Steps;
using K1vs.DotNetPipe.Universal.Steps.HandlerSteps;
using K1vs.DotNetPipe.Universal.Steps.IfElseSteps;
using K1vs.DotNetPipe.Universal.Steps.IfSteps;
using K1vs.DotNetPipe.Universal.Steps.LinearSteps;
using K1vs.DotNetPipe.Universal.Steps.ForkSteps;
using K1vs.DotNetPipe.Universal.Steps.MultiForkSteps;
using K1vs.DotNetPipe.Universal.Steps.SwitchSteps;
using System.Collections.Immutable;
using K1vs.DotNetPipe.Exceptions;

namespace K1vs.DotNetPipe.Universal;

/// <summary>
/// Space is a container for pipelines and steps.
/// It allows you to create and manage pipelines, and provides methods to retrieve pipelines and steps by their names.
/// Retrieved steps can be modified by mutators, which can change the way steps are executed or how data is processed.
/// Each space can contain multiple pipelines, and each pipeline can have multiple steps.
/// </summary>
public class Space
{
    private readonly Dictionary<StepName, Step> _steps = [];

    private readonly Dictionary<string, IPipeline> _pipelines = [];

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
    public IReadOnlyDictionary<string, IPipeline> Pipelines => _pipelines.AsReadOnly();

    /// <summary>
    /// Stating point for creating a new pipeline.
    /// This method initializes a new pipeline with the specified name.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>A new pipeline entry.</returns>
    public PipelineEntry<TInput> CreatePipeline<TInput>(string name)
    {
        var builder = new PipelineBuilder(this, name);
        return new PipelineEntry<TInput>(builder);
    }

    /// <summary>
    /// Retrieves a pipeline by its name.
    /// If the pipeline does not exist, it returns null.
    /// </summary>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The pipeline if found; otherwise, null.</returns>
    public IPipeline? GetPipeline(string name)
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
    public IPipeline? GetRequiredPipeline(string name)
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
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The pipeline if found and of the correct type; otherwise, null.</returns>
    public Pipeline<TInput>? GetPipeline<TInput>(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline) && pipeline is Pipeline<TInput> closedPipeline)
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
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The pipeline if found and of the correct type; otherwise, null.</returns>
    /// <exception cref="PipelineNotFoundException"></exception>
    /// <exception cref="UnexpectedPipelineTypeException"></exception>
    public Pipeline<TInput> GetRequiredPipeline<TInput>(string name)
    {
        if (!_pipelines.TryGetValue(name, out var pipeline))
        {
            throw new PipelineNotFoundException(name);
        }
        if (pipeline is not Pipeline<TInput> typedPipeline)
        {
            throw new UnexpectedPipelineTypeException(name, pipeline.GetType(), typeof(Pipeline<TInput>));
        }
        return typedPipeline;
    }

    /// <summary>
    /// Retrieves an open pipeline by its name and casts it to the specified type.
    /// If the pipeline does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <typeparam name="TNextInput">The type of the next input data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The open pipeline if found and of the correct type; otherwise, null.</returns>
    public OpenPipeline<TInput, TNextInput>? GetOpenPipeline<TInput, TNextInput>(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline) && pipeline is OpenPipeline<TInput, TNextInput> openPipeline)
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
    /// <typeparam name="TNextInput">The type of the next input data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The open pipeline if found and of the correct type; otherwise, null.</returns>
    /// <exception cref="PipelineNotFoundException"></exception>
    /// <exception cref="UnexpectedPipelineTypeException"></exception>
    public OpenPipeline<TInput, TNextInput> GetRequiredOpenPipeline<TInput, TNextInput>(string name)
    {
        if (!_pipelines.TryGetValue(name, out var pipeline))
        {
            throw new PipelineNotFoundException(name);
        }
        if (pipeline is not OpenPipeline<TInput, TNextInput> openPipeline)
        {
            throw new UnexpectedPipelineTypeException(name, pipeline.GetType(), typeof(OpenPipeline<TInput, TNextInput>));
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
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TNextInput">The type of the next step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The linear step if found and of the correct type; otherwise, null.</returns>
    public LinearStep<TEntryStepInput, TInput, TNextInput>? GetLinearStep<TEntryStepInput, TInput, TNextInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is LinearStep<TEntryStepInput, TInput, TNextInput> pipelineStep)
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
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TNextInput">The type of the next step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The linear step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public LinearStep<TEntryStepInput, TInput, TNextInput> GetRequiredLinearStep<TEntryStepInput, TInput, TNextInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not LinearStep<TEntryStepInput, TInput, TNextInput> pipelineStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(LinearStep<TEntryStepInput, TInput, TNextInput>));
        }
        return pipelineStep;
    }

    /// <summary>
    /// Retrieves a handler step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The handler step if found and of the correct type; otherwise, null.</returns>
    public HandlerStep<TEntryStepInput, TInput>? GetHandlerStep<TEntryStepInput, TInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is HandlerStep<TEntryStepInput, TInput> pipelineTerminator)
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
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The handler step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public HandlerStep<TEntryStepInput, TInput> GetRequiredHandlerStep<TEntryStepInput, TInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not HandlerStep<TEntryStepInput, TInput> pipelineTerminator)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(HandlerStep<TEntryStepInput, TInput>));
        }
        return pipelineTerminator;
    }

    /// <summary>
    /// Retrieves an if step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TIfInput">The type of the if step input.</typeparam>
    /// <typeparam name="TNextInput">The type of the next step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The if step if found and of the correct type; otherwise, null.</returns>
    public IfStep<TEntryStepInput, TInput, TIfInput, TNextInput>? GetIfStep<TEntryStepInput, TInput, TIfInput, TNextInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is IfStep<TEntryStepInput, TInput, TIfInput, TNextInput> pipelineIfStep)
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
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TIfInput">The type of the if step input.</typeparam>
    /// <typeparam name="TNextInput">The type of the next step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The if step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public IfStep<TEntryStepInput, TInput, TIfInput, TNextInput> GetRequiredIfStep<TEntryStepInput, TInput, TIfInput, TNextInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not IfStep<TEntryStepInput, TInput, TIfInput, TNextInput> pipelineIfStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(IfStep<TEntryStepInput, TInput, TIfInput, TNextInput>));
        }
        return pipelineIfStep;
    }

    /// <summary>
    /// Retrieves an if-else step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TIfInput">The type of the if step input.</typeparam>
    /// <typeparam name="TElseInput">The type of the else step input.</typeparam>
    /// <typeparam name="TNextInput">The type of the next step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The if-else step if found and of the correct type; otherwise, null.</returns>
    public IfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextInput>? GetIfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is IfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextInput> pipelineIfElseStep)
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
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TIfInput">The type of the if step input.</typeparam>
    /// <typeparam name="TElseInput">The type of the else step input.</typeparam>
    /// <typeparam name="TNextInput">The type of the next step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The if-else step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public IfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextInput> GetRequiredIfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not IfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextInput> pipelineIfElseStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(IfElseStep<TEntryStepInput, TInput, TIfInput, TElseInput, TNextInput>));
        }
        return pipelineIfElseStep;
    }

    /// <summary>
    /// Retrieves a fork step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TBranchAInput">The type of the branch A input.</typeparam>
    /// <typeparam name="TBranchBInput">The type of the branch B input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The fork step if found and of the correct type; otherwise, null.</returns>
    public ForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput>? GetForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is ForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput> forkStep)
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
    /// <typeparam name="TEntryStepInput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TBranchAInput"></typeparam>
    /// <typeparam name="TBranchBInput"></typeparam>
    /// <param name="pipeline"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public ForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput> GetRequiredForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not ForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput> forkStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(ForkStep<TEntryStepInput, TInput, TBranchAInput, TBranchBInput>));
        }
        return forkStep;
    }

    /// <summary>
    /// Retrieves a multi-fork step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TBranchesInput">The type of the branches input.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the default input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The multi-fork step if found and of the correct type; otherwise, null.</returns>
    public MultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput>? GetMultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is MultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput> multiForkStep)
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
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TBranchesInput">The type of the branches input.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the default input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The multi-fork step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public MultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput> GetRequiredMultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not MultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput> multiForkStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(MultiForkStep<TEntryStepInput, TInput, TBranchesInput, TDefaultInput>));
        }
        return multiForkStep;
    }

    /// <summary>
    /// Retrieves a switch step by its name within a specific pipeline.
    /// If the step does not exist or is of a different type, it returns null.
    /// </summary>
    /// <typeparam name="TEntryStepInput">The type of the entry step input.</typeparam>
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TCaseInput">The type of the case input.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the default input.</typeparam>
    /// <typeparam name="TNextStepInput">The type of the next step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The switch step if found and of the correct type; otherwise, null.</returns>
    public SwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>? GetSwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is SwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> switchStep)
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
    /// <typeparam name="TInput">The type of the step input.</typeparam>
    /// <typeparam name="TCaseInput">The type of the case input.</typeparam>
    /// <typeparam name="TDefaultInput">The type of the default input.</typeparam>
    /// <typeparam name="TNextStepInput">The type of the next step input.</typeparam>
    /// <param name="pipeline">The name of the pipeline.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>The switch step if found and of the correct type; otherwise, throws an exception.</returns>
    /// <exception cref="StepNotFoundException"></exception>
    /// <exception cref="UnexpectedStepTypeException"></exception>
    public SwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> GetRequiredSwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not SwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> switchStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(SwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>));
        }
        return switchStep;
    }

    /// <summary>
    /// Adds a pipeline to the space.
    /// If a pipeline with the same name already exists, it throws a PipelineWithNameAlreadyExistsException.
    /// </summary>
    /// <param name="pipeline">The pipeline to add.</param>
    /// <exception cref="PipelineWithNameAlreadyExistsException"></exception>
    internal void AddPipeline(IPipeline pipeline)
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
