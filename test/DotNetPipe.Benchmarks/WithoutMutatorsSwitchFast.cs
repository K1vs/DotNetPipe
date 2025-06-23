using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsSwitchFast;

namespace DotNetPipe.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class WithoutMutatorsSwitchFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClass _virtualClassHandler;

    private readonly DotNetPipePipeline _pipelineHandler;

    public WithoutMutatorsSwitchFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClass();
    }

    [Params("105", "50", "-5", "0", "abc", "hello", "")]
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
