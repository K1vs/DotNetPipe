using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsForkFast;

namespace DotNetPipe.Benchmarks.Async;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class AsyncWithMutatorsForkFast
{
    private readonly NotVirtualClass _classHandler;
    private readonly VirtualClassWithMutation _virtualClassHandler;
    private readonly DotNetPipePipeline _pipelineHandler;

    public AsyncWithMutatorsForkFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClassWithMutation();
    }

    [Params("123", "  456  ", "abc123def", "hello", "", "!@#")]
    public string Input { get; set; }

    [Benchmark]
    public async Task Pipeline()
    {
        await _pipelineHandler.Run(Input);
    }

    [Benchmark]
    public async Task Class()
    {
        await _classHandler.Run(Input);
    }

    [Benchmark]
    public async Task VirtualClass()
    {
        await _virtualClassHandler.Run(Input);
    }
}
