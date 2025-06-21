using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsIfElseTwoStepsFast;

namespace DotNetPipe.Benchmarks;

[MemoryDiagnoser]
public class WithoutMutatorsIfElseTwoStepsFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClass _virtualClassHandler = new();

    private readonly DotNetPipePipeline _pipelineHandler;

    public WithoutMutatorsIfElseTwoStepsFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClass();
    }

    [Params(-25, 0, 100)]
    public int Input { get; set; }

    [Benchmark]
    public async ValueTask Pipeline()
    {
        await _pipelineHandler.Run(Input);
    }

    [Benchmark]
    public async ValueTask Class()
    {
        await _classHandler.Run(Input);
    }

    [Benchmark]
    public async ValueTask VirtualClass()
    {
        await _virtualClassHandler.Run(Input);
    }
}
