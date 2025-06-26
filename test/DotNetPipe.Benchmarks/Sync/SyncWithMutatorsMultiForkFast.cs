using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsMultiForkFast;

namespace DotNetPipe.Benchmarks.Sync;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class SyncWithMutatorsMultiForkFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClassWithMutation _virtualClassHandler;

    private readonly DotNetPipePipeline _pipelineHandler;

    public SyncWithMutatorsMultiForkFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClassWithMutation();
    }

    [Params("123", "abc", "!@#", "a1b2", "hello123", "")]
    public string Input { get; set; }

    [Benchmark]
    public void Pipeline()
    {
        _pipelineHandler.Run(Input);
    }

    [Benchmark]
    public void Class()
    {
        _classHandler.Run(Input);
    }

    [Benchmark]
    public void VirtualClass()
    {
        _virtualClassHandler.Run(Input);
    }
}
