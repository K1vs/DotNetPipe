using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsMultiForkFast;

namespace DotNetPipe.Benchmarks;

[MemoryDiagnoser]
public class WithoutMutatorsMultiForkFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClass _virtualClassHandler = new();

    private readonly DotNetPipePipeline _pipelineHandler;

    public WithoutMutatorsMultiForkFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClass();
    }

    [Params("123", "abc", "!@#", "a1b2", "hello123", "")]
    public string Input { get; set; }

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
