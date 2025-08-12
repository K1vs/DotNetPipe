namespace K1vs.DotNetPipe.ReturningAsyncCancellable.Steps;

/// <summary>
/// Represents a step name in the format "PipelineName_StepName".
/// This is used to uniquely identify a step in space.
/// </summary>
/// <param name="Name">The name of the step.</param>
/// <param name="PipelineName">The name of the pipeline to which the step belongs.</param>
public record StepName(string Name, string PipelineName)
{
    /// <summary>
    /// Returns a string representation of the step name in the format "{PipelineName}_{Name}".
    /// </summary>
    /// <returns>A formatted string representing the step name.</returns>
    public override string ToString()
    {
        return $"{PipelineName}_{Name}";
    }
}



