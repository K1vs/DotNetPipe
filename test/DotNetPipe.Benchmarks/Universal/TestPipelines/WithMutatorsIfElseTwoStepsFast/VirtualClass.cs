using System.Threading.Tasks;
using BenchmarkDotNet.Engines;

namespace DotNetPipe.Benchmarks.Universal.TestPipelines.WithMutatorsIfElseTwoStepsFast;

internal class VirtualClass
{
    protected const int _multiplier = -2;

    private readonly Consumer _consumer = new();

    public virtual async ValueTask Run(int input)
    {
        var result = input;

        // CheckNegative: input < 0
        if (CheckNegative(input))
        {
            // MultiplyByConstant
            result = MultiplyByConstant(input);
        }

        // ConsumeNumber handler
        await ConsumeNumber(result);
    }

    protected virtual bool CheckNegative(int input)
    {
        return input < 0;
    }

    protected virtual int MultiplyByConstant(int input)
    {
        return input * _multiplier;
    }

    protected virtual async ValueTask ConsumeNumber(int input)
    {
        _consumer.Consume(input);
        await ValueTask.CompletedTask;
    }
}

internal class VirtualClassWithMutation : VirtualClass
{
    // Override CheckNegative to use <= 0 instead of < 0
    protected override bool CheckNegative(int input)
    {
        return input <= 0;
    }

    // Override MultiplyByConstant to add 1 before multiplying
    protected override int MultiplyByConstant(int input)
    {
        return (input + 1) * _multiplier;
    }
}
