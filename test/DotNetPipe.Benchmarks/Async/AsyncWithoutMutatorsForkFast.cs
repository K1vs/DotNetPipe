using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.Async.TestPipelines.WithoutMutatorsForkFast;

namespace DotNetPipe.Benchmarks.Async;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class AsyncWithoutMutatorsForkFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClass _virtualClassHandler;

    private readonly DotNetPipePipeline _pipelineHandler;

    public AsyncWithoutMutatorsForkFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClass();
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
