using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.HandlerSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.IfElseSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.IfSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.LinearSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.ForkSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.MultiForkSteps;
using K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.SwitchSteps;
using K1vs.DotNetPipe.Exceptions;

namespace K1vs.DotNetPipe.ReturningAsyncCancellable;

/// <summary>
/// Space is a container for pipelines and steps for returning pipelines with cancellation support.
/// It allows creating and managing pipelines, and provides methods to retrieve pipelines and steps by their names.
/// Retrieved steps can be modified by mutators.
/// </summary>
public class Space
{
    private readonly Dictionary<StepName, Step> _steps = [];
    private readonly Dictionary<string, Pipeline> _pipelines = [];

    /// <summary>
    /// Gets a read-only collection of all steps in the space.
    /// </summary>
    public IReadOnlyDictionary<StepName, Step> Steps => _steps.AsReadOnly();
    /// <summary>
    /// Gets a read-only collection of all pipelines in the space.
    /// </summary>
    public IReadOnlyDictionary<string, Pipeline> Pipelines => _pipelines.AsReadOnly();

    /// <summary>
    /// Starting point for creating a new pipeline with the specified name.
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
    /// </summary>
    public Pipeline? GetPipeline(string name) => _pipelines.GetValueOrDefault(name);

    /// <summary>
    /// Retrieves a pipeline by its name. Throws if not found.
    /// </summary>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>The pipeline instance.</returns>
    public Pipeline? GetRequiredPipeline(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline))
        {
            return pipeline;
        }
        throw new PipelineNotFoundException(name);
    }

    /// <summary>
    /// Retrieves a typed closed pipeline by its name.
    /// Returns null if missing or type mismatched.
    /// </summary>
    public Pipeline<TInput, TOutput>? GetPipeline<TInput, TOutput>(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline) && pipeline is Pipeline<TInput, TOutput> closedPipeline)
        {
            return closedPipeline;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a typed pipeline by name. Throws when missing or type mismatched.
    /// </summary>
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
    public OpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult>? GetOpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult>(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline) && pipeline is OpenPipeline<TInput, TInputResult, TNextInput, TNextInputResult> openPipeline)
        {
            return openPipeline;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a typed open pipeline by name. Throws when missing or type mismatched.
    /// </summary>
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
    public Step? GetStep(string pipeline, string name) => _steps.GetValueOrDefault(new StepName(name, pipeline));

    /// <summary>
    /// Retrieves a step by its name within a specific pipeline. Throws when not found.
    /// </summary>
    public Step GetRequiredStep(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        return step;
    }

    /// <summary>
    /// Retrieves a typed linear step by name within a pipeline.
    /// Returns null if missing or type mismatched.
    /// </summary>
    public LinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult>? GetLinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is LinearStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TNextInput, TNextResult> pipelineStep)
        {
            return pipelineStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a typed linear step by name within a pipeline.
    /// Throws when missing or type mismatched.
    /// </summary>
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
    /// Retrieves a typed handler step by name within a pipeline.
    /// Returns null if missing or type mismatched.
    /// </summary>
    public HandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult>? GetHandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is HandlerStep<TEntryStepInput, TEntryStepResult, TInput, TResult> pipelineTerminator)
        {
            return pipelineTerminator;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a typed handler step by name within a pipeline.
    /// Throws when missing or type mismatched.
    /// </summary>
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
    /// Retrieves a typed if step by name within a pipeline.
    /// Returns null if missing or type mismatched.
    /// </summary>
    public IfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult>? GetIfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is IfStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TNextStepInput, TNextStepResult> pipelineIfStep)
        {
            return pipelineIfStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a typed if step by name within a pipeline.
    /// Throws when missing or type mismatched.
    /// </summary>
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
    /// Retrieves a typed if-else step by name within a pipeline.
    /// Returns null if missing or type mismatched.
    /// </summary>
    public IfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult>? GetIfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is IfElseStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextStepInput, TNextStepResult> pipelineIfElseStep)
        {
            return pipelineIfElseStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a typed if-else step by name within a pipeline.
    /// Throws when missing or type mismatched.
    /// </summary>
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
    /// Retrieves a typed fork step by name within a pipeline.
    /// Returns null if missing or type mismatched.
    /// </summary>
    public ForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>? GetForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is ForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> forkStep)
        {
            return forkStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a typed fork step by name within a pipeline.
    /// Throws when missing or type mismatched.
    /// </summary>
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
    /// Retrieves a typed multi-fork step by name within a pipeline.
    /// Returns null if missing or type mismatched.
    /// </summary>
    public MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>? GetMultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is MultiForkStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> multiForkStep)
        {
            return multiForkStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a typed multi-fork step by name within a pipeline.
    /// Throws when missing or type mismatched.
    /// </summary>
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
    /// Retrieves a typed switch step by name within a pipeline.
    /// Returns null if missing or type mismatched.
    /// </summary>
    public SwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult>? GetSwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is SwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> switchStep)
        {
            return switchStep;
        }
        return null;
    }

    /// <summary>
    /// Retrieves a typed switch step by name within a pipeline.
    /// Throws when missing or type mismatched.
    /// </summary>
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

    internal void AddPipeline(Pipeline pipeline)
    {
        if (!_pipelines.TryAdd(pipeline.Name, pipeline))
        {
            throw new PipelineWithNameAlreadyExistsException(pipeline.Name);
        }
    }

    internal void AddStep(Step step)
    {
        if (!_steps.TryAdd(step.Name, step))
        {
            throw new StepWithNameAlreadyExistsException(step.Name.ToString());
        }
    }
}



