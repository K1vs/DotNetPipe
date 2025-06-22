namespace K1vs.DotNetPipe;

public static class Pipelines
{
    public static Universal.PipelineEntry<TInput> CreatePipeline<TInput>(string name)
    {
        var space = new Universal.Space();
        var pipeline = space.CreatePipeline<TInput>(name);
        return pipeline;
    }

    public static Universal.Space CreateSpace()
    {
        return new Universal.Space();
    }
}