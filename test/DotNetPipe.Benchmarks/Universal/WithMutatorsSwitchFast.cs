using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsSwitchFast;

namespace DotNetPipe.Benchmarks.Universal;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class WithMutatorsSwitchFast
{
    private readonly NotVirtualClass _classHandler;
    private readonly VirtualClassWithMutation _virtualClassHandler;
    private readonly DotNetPipePipeline _pipelineHandler;

    public WithMutatorsSwitchFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClassWithMutation();
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
