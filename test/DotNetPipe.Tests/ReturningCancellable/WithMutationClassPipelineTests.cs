using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.ReturningCancellable;

namespace K1vs.DotNetPipe.Tests.ReturningCancellable;

public class WithMutationClassReturningPipelineTests
{
    [Theory]
    [InlineData(-4, -3)]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStep_ShouldReturn(int value, int expected)
    {
        var test = new TestReturningCancellableClassPipeline([
            new TestReturningCancellableClassHandlerMutator()
        ]);
        var result = await test.Run(value, CancellationToken.None);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-4, 3)]
    [InlineData(0, 11)]
    [InlineData(2, 15)]
    public async Task BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldReturn(int input, int expected)
    {
        var test = new TestReturningCancellableClassTwoStepPipeline([
            new TestReturningCancellableClassLinearMutator(),
            new TestReturningCancellableClassTwoStepHandlerMutator()
        ]);
        var result = await test.Run(input, CancellationToken.None);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(2, 45)]
    [InlineData(0, 41)]
    [InlineData(-1, 39)]
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldReturn(int input, int expected)
    {
        var test = new TestReturningCancellableClassThreeStepPipeline([
            new TestReturningCancellableClassFirstLinearMutator(),
            new TestReturningCancellableClassSecondLinearMutator(),
            new TestReturningCancellableClassThreeStepHandlerMutator()
        ]);
        var result = await test.Run(input, CancellationToken.None);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("  5  ", 8)]
    [InlineData(" 10 ", 13)]
    [InlineData("3.7", 7)]
    [InlineData(" 2.3 ", 5)]
    [InlineData("5.5", 8)]
    public async Task BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldReturn(string inputValue, int expected)
    {
        var test = new TestReturningCancellableClassIfStepPipeline([
            new TestReturningCancellableClassIfTrimMutator(),
            new TestReturningCancellableClassIfSelectorMutator(),
            new TestReturningCancellableClassIfRoundToIntMutator(),
            new TestReturningCancellableClassIfAddConstantMutator(),
            new TestReturningCancellableClassIfHandlerMutator()
        ]);
        var result = await test.Run(inputValue, CancellationToken.None);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("  5  ", 10)]
    [InlineData(" 10 ", 15)]
    [InlineData("3.7", 8)]
    [InlineData(" 2.3 ", 8)]
    [InlineData("5.5", 8)]
    public async Task BuildAndRunPipeline_WhenIfElseStepHandlesIntFloat_ShouldReturn(string inputValue, int expected)
    {
        var test = new TestReturningCancellableClassIfElseStepPipeline([
            new TestReturningCancellableClassIfElseTrimMutator(),
            new TestReturningCancellableClassIfElseSelectorMutator(),
            new TestReturningCancellableClassIfElseParseFloatMutator(),
            new TestReturningCancellableClassIfElseRoundToIntMutator(),
            new TestReturningCancellableClassIfElseMultiplyMutator(),
            new TestReturningCancellableClassIfElseAddConstantMutator(),
            new TestReturningCancellableClassIfElseHandlerMutator()
        ]);
        var result = await test.Run(inputValue, CancellationToken.None);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(" 105 ", 318)]
    [InlineData(" 50 ", 55)]
    [InlineData(" -5 ", -7)]
    [InlineData(" 0 ", 3)]
    [InlineData("abc", 6)]
    [InlineData("hello", 8)]
    [InlineData("", 3)]
    public async Task BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldReturn(string inputValue, int expected)
    {
        var test = new TestReturningCancellableClassSwitchStepPipeline([
            new TestReturningCancellableClassSwitchTrimMutator(),
            new TestReturningCancellableClassSwitchSelectorMutator(),
            new TestReturningCancellableClassSwitchMultiplyByThreeMutator(),
            new TestReturningCancellableClassSwitchAddTwoMutator(),
            new TestReturningCancellableClassSwitchMultiplyByTwoMutator(),
            new TestReturningCancellableClassSwitchKeepZeroMutator(),
            new TestReturningCancellableClassSwitchStringLengthMutator(),
            new TestReturningCancellableClassSwitchHandlerMutator()
        ]);
        var result = await test.Run(inputValue, CancellationToken.None);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", 0, "******")]
    [InlineData("  456  ", 0, "******")]
    [InlineData("abc", 0, "***abc***")]
    [InlineData("hello", 5, null)]
    [InlineData("", 0, "******")]
    [InlineData("!@#", 0, "***!@#***")]
    public async Task BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldReturn(string inputValue, int expectedInt, string? expectedString)
    {
        var test = new TestReturningCancellableClassForkStepPipeline([
            new TestReturningCancellableClassForkTrimMutator(),
            new TestReturningCancellableClassForkSelectorMutator(),
            new TestReturningCancellableClassForkRemoveNonDigitsMutator(),
            new TestReturningCancellableClassForkParseToIntMutator(),
            new TestReturningCancellableClassForkIntHandlerMutator(),
            new TestReturningCancellableClassForkRemoveDigitsMutator(),
            new TestReturningCancellableClassForkAddSpacesMutator(),
            new TestReturningCancellableClassForkStringHandlerMutator()
        ]);
        var result = await test.Run(inputValue, CancellationToken.None);
        if (expectedString == null)
        {
            Assert.Equal(expectedInt, result.Item1);
            Assert.Null(result.Item2);
        }
        else
        {
            Assert.Null(result.Item1);
            Assert.Equal(expectedString, result.Item2);
        }
    }

    [Theory]
    [InlineData("123", 138, "", new char[0])]
    [InlineData("  456  ", 471, "", new char[0])]
    [InlineData("abc", 0, "", new char[] { 'a', 'b', 'c', '_' })]
    [InlineData("xyz", 0, "", new char[] { 'x', 'y', 'z', '_' })]
    [InlineData("!@#", 0, "", new char[] { '!', '@', '#', '_' })]
    [InlineData("@@@", 0, "", new char[] { '@', '_' })]
    [InlineData("a1b2", 0, "", new char[] { 'a', '1', 'b', '2', '_' })]
    [InlineData("hello123", 0, "", new char[] { 'h', 'e', 'l', 'o', '1', '2', '3', '_' })]
    [InlineData("12345abc", 0, "", new char[] { '1', '2', '3', '4', '5', 'a', 'b', 'c', '_' })]
    public async Task BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldReturn(
        string inputValue,
        int expectedIntResult,
        string expectedStringResult,
        char[] expectedCharArrayResult)
    {
        var test = new TestReturningCancellableClassMultiForkStepPipeline(new IMutator<Space>[]
        {
            new TestReturningCancellableClassMultiForkTrimMutator(),
            new TestReturningCancellableClassMultiForkSelectorMutator(),
            new TestReturningCancellableClassMultiForkParseToIntMutator(),
            new TestReturningCancellableClassMultiForkAddConstantMutator(),
            new TestReturningCancellableClassMultiForkIntHandlerMutator(),
            new TestReturningCancellableClassMultiForkAddSpacesMutator(),
            new TestReturningCancellableClassMultiForkStringHandlerMutator(),
            new TestReturningCancellableClassMultiForkRemoveWhitespaceMutator(),
            new TestReturningCancellableClassMultiForkConvertToCharArrayMutator(),
            new TestReturningCancellableClassMultiForkRemoveDuplicatesMutator(),
            new TestReturningCancellableClassMultiForkCountDigitsAndLettersMutator(),
            new TestReturningCancellableClassMultiForkCalculateRatioMutator(),
            new TestReturningCancellableClassMultiForkDefaultIntHandlerMutator()
        });

        var result = await test.Run(inputValue, CancellationToken.None);
        if (expectedStringResult != "")
        {
            Assert.Null(result.Item1);
            Assert.Equal(expectedStringResult, result.Item2);
            Assert.Null(result.Item3);
        }
        else if (expectedCharArrayResult.Length > 0)
        {
            Assert.Null(result.Item1);
            Assert.Null(result.Item2);
            Assert.True(result.Item3!.SequenceEqual(expectedCharArrayResult));
        }
        else
        {
            Assert.Equal(expectedIntResult, result.Item1);
            Assert.Null(result.Item2);
            Assert.Null(result.Item3);
        }
    }
}

// Below are class-based implementations and mutators adapted for ReturningCancellable.

public class TestReturningCancellableClassHandlerStep : IHandlerStep<int, int>
{
    public string Name => "TestHandler";
    public ValueTask<int> Handle(int input, CancellationToken ct = default) => ValueTask.FromResult(input);
}

public class TestReturningCancellableClassLinearStep : ILinearStep<int, int, int, int>
{
    private const int ConstantToAdd = 10;
    public string Name => "AddConstant";
    public async ValueTask<int> Handle(int input, Handler<int, int> next, CancellationToken ct = default)
    {
        var result = input + ConstantToAdd;
        return await next(result, ct);
    }
}

public class TestReturningCancellableClassSecondLinearStep : ILinearStep<int, int, int, int>
{
    private const int MultiplierCoefficient = 2;
    public string Name => "MultiplyByCoefficient";
    public async ValueTask<int> Handle(int input, Handler<int, int> next, CancellationToken ct = default)
    {
        var result = input * MultiplierCoefficient;
        return await next(result, ct);
    }
}

public class TestReturningCancellableClassTrimStep : ILinearStep<string, int, string, int>
{
    public string Name => "TrimString";
    public async ValueTask<int> Handle(string input, Handler<string, int> next, CancellationToken ct = default)
    {
        return await next(input.Trim(), ct);
    }
}

public class TestReturningCancellableClassIfStep : IIfStep<string, int, string, int, int, int>
{
    public string Name => "CheckIntOrFloat";
    public async ValueTask<int> Handle(string input, Handler<string, int> ifNext, Handler<int, int> next, CancellationToken ct = default)
    {
        if (int.TryParse(input, out var intValue))
        {
            return await next(intValue, ct);
        }
        return await ifNext(input, ct);
    }
    public OpenPipeline<string, int, int, int> BuildTruePipeline(Space space)
    {
        return space.CreatePipeline<string, int>("FloatProcessing")
            .StartWithLinear<double, int>("ParseFloat", async (val, nxt, ct) =>
            {
                if (double.TryParse(val, out var f)) return await nxt(f, ct);
                return 0;
            })
            .ThenLinear<int, int>("RoundToInt", async (val, nxt, ct) =>
            {
                var rounded = (int)Math.Round(val);
                return await nxt(rounded, ct);
            })
            .BuildOpenPipeline();
    }
}

public class TestReturningCancellableClassIfAddConstantStep : ILinearStep<int, int, int, int>
{
    public string Name => "AddConstant";
    public async ValueTask<int> Handle(int input, Handler<int, int> next, CancellationToken ct = default)
    {
        var result = input + 2;
        return await next(result, ct);
    }
}

public class TestReturningCancellableClassIfHandlerStep : IHandlerStep<int, int>
{
    public string Name => "TestHandler";
    public ValueTask<int> Handle(int input, CancellationToken ct = default) => ValueTask.FromResult(input);
}

public class TestReturningCancellableClassIfElseStep : IIfElseStep<string, int, string, int, int, int, int, int>
{
    public string Name => "CheckIntOrFloat";
    public async ValueTask<int> Handle(string input, Handler<string, int> ifNext, Handler<int, int> elseNext, CancellationToken ct = default)
    {
        if (int.TryParse(input, out var intValue))
        {
            return await elseNext(intValue, ct);
        }
        return await ifNext(input, ct);
    }
    public OpenPipeline<string, int, int, int> BuildTruePipeline(Space space)
    {
        return space.CreatePipeline<string, int>("FloatProcessing")
            .StartWithLinear<double, int>("ParseFloat", async (val, nxt, ct) =>
            {
                if (double.TryParse(val, out var f)) return await nxt(f, ct);
                return 0;
            })
            .ThenLinear<int, int>("RoundToInt", async (val, nxt, ct) =>
            {
                var rounded = (int)Math.Round(val);
                return await nxt(rounded, ct);
            })
            .BuildOpenPipeline();
    }
    public OpenPipeline<int, int, int, int> BuildFalsePipeline(Space space)
    {
        return space.CreatePipeline<int, int>("IntProcessing")
            .StartWithLinear<int, int>("Multiply", async (val, nxt, ct) => await nxt(val * 2, ct))
            .BuildOpenPipeline();
    }
}

public class TestReturningCancellableClassIfElseAddConstantStep : ILinearStep<int, int, int, int>
{
    public string Name => "AddConstant";
    public async ValueTask<int> Handle(int input, Handler<int, int> next, CancellationToken ct = default)
    {
        var result = input + 3;
        return await next(result, ct);
    }
}

public class TestReturningCancellableClassIfElseHandlerStep : IHandlerStep<int, int>
{
    public string Name => "TestHandler";
    public ValueTask<int> Handle(int input, CancellationToken ct = default) => ValueTask.FromResult(input);
}

public class TestReturningCancellableClassNumberRangeSwitchStep : ISwitchStep<string, int, int, int, int, int, int, int>
{
    public string Name => "NumberRangeSwitch";
    public async ValueTask<int> Handle(string input, IReadOnlyDictionary<string, Handler<int, int>> cases, Handler<int, int> defaultNext, CancellationToken ct = default)
    {
        if (int.TryParse(input, out var number))
        {
            if (number > 100) return await cases["GreaterThan100"](number, ct);
            if (number > 0) return await cases["BetweenZeroAndHundred"](number, ct);
            if (number < 0) return await cases["LessThanZero"](number, ct);
            return await cases["EqualToZero"](number, ct);
        }
        return await defaultNext(input.Length, ct);
    }
    public IReadOnlyDictionary<string, OpenPipeline<int, int, int, int>> BuildCasesPipelines(Space space)
    {
        return new Dictionary<string, OpenPipeline<int, int, int, int>>
        {
            ["GreaterThan100"] = space.CreatePipeline<int, int>("MultiplyByThree")
                .StartWithLinear<int, int>("MultiplyOperation", async (input, next, ct) => await next(input * 3, ct))
                .BuildOpenPipeline(),
            ["BetweenZeroAndHundred"] = space.CreatePipeline<int, int>("AddTwo")
                .StartWithLinear<int, int>("AddOperation", async (input, next, ct) => await next(input + 2, ct))
                .BuildOpenPipeline(),
            ["LessThanZero"] = space.CreatePipeline<int, int>("MultiplyByTwo")
                .StartWithLinear<int, int>("MultiplyOperation", async (input, next, ct) => await next(input * 2, ct))
                .BuildOpenPipeline(),
            ["EqualToZero"] = space.CreatePipeline<int, int>("KeepZero")
                .StartWithLinear<int, int>("IdentityOperation", async (input, next, ct) => await next(input, ct))
                .BuildOpenPipeline()
        }.AsReadOnly();
    }
    public OpenPipeline<int, int, int, int> BuildDefaultPipeline(Space space)
    {
        return space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", async (input, next, ct) => await next(input, ct))
            .BuildOpenPipeline();
    }
}

public class TestReturningCancellableClassPipeline
{
    private readonly Handler<int, int> _compiled;
    public TestReturningCancellableClassPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var handler = new TestReturningCancellableClassHandlerStep();
        var pipeline = new Space()
            .CreatePipeline<int, int>("TestPipeline")
            .StartWithHandler(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public ValueTask<int> Run(int input, CancellationToken ct) => _compiled(input, ct);
}

public class TestReturningCancellableClassTwoStepPipeline
{
    private readonly Handler<int, int> _compiled;
    public TestReturningCancellableClassTwoStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var linear = new TestReturningCancellableClassLinearStep();
        var handler = new TestReturningCancellableClassHandlerStep();
        var pipeline = new Space()
            .CreatePipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear(linear)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public ValueTask<int> Run(int input, CancellationToken ct) => _compiled(input, ct);
}

public class TestReturningCancellableClassThreeStepPipeline
{
    private readonly Handler<int, int> _compiled;
    public TestReturningCancellableClassThreeStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var first = new TestReturningCancellableClassLinearStep();
        var second = new TestReturningCancellableClassSecondLinearStep();
        var handler = new TestReturningCancellableClassHandlerStep();
        var pipeline = new Space()
            .CreatePipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear(first)
            .ThenLinear(second)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public ValueTask<int> Run(int input, CancellationToken ct) => _compiled(input, ct);
}

public class TestReturningCancellableClassIfStepPipeline
{
    private readonly Handler<string, int> _compiled;
    public TestReturningCancellableClassIfStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var trim = new TestReturningCancellableClassTrimStep();
        var ifStep = new TestReturningCancellableClassIfStep();
        var add = new TestReturningCancellableClassIfAddConstantStep();
        var handler = new TestReturningCancellableClassIfHandlerStep();
        var pipeline = new Space()
            .CreatePipeline<string, int>("TestIfStepPipeline")
            .StartWithLinear(trim)
            .ThenIf(ifStep)
            .ThenLinear(add)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public ValueTask<int> Run(string input, CancellationToken ct) => _compiled(input, ct);
}

public class TestReturningCancellableClassIfElseStepPipeline
{
    private readonly Handler<string, int> _compiled;
    public TestReturningCancellableClassIfElseStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var trim = new TestReturningCancellableClassTrimStep();
        var ifElse = new TestReturningCancellableClassIfElseStep();
        var add = new TestReturningCancellableClassIfElseAddConstantStep();
        var handler = new TestReturningCancellableClassIfElseHandlerStep();
        var pipeline = new Space()
            .CreatePipeline<string, int>("TestIfElseStepPipeline")
            .StartWithLinear(trim)
            .ThenIfElse(ifElse)
            .ThenLinear(add)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public ValueTask<int> Run(string input, CancellationToken ct) => _compiled(input, ct);
}

public class TestReturningCancellableClassSwitchStepPipeline
{
    private readonly Handler<string, int> _compiled;
    public TestReturningCancellableClassSwitchStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var trim = new TestReturningCancellableClassTrimStep();
        var switchStep = new TestReturningCancellableClassNumberRangeSwitchStep();
        var handler = new TestReturningCancellableClassIfHandlerStep();
        var space = new Space();
        var pipeline = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear(trim)
            .ThenSwitch(switchStep)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public ValueTask<int> Run(string input, CancellationToken ct) => _compiled(input, ct);
}

// Mutators (class-based)

public class TestReturningCancellableClassHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int, int, int>("TestPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async (input, ct) => await handler(input + 1, ct));
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassTwoStepHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int, int, int>("TestTwoStepPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async (input, ct) => await handler(input + 1, ct));
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestTwoStepPipeline", "AddConstant");
        var mutator = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => async (input, next, ct) => await pipe(input * 2, next, ct));
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassFirstLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestThreeStepPipeline", "AddConstant");
        var mutator = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => async (input, next, ct) => await pipe(input + 5, next, ct));
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassSecondLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestThreeStepPipeline", "MultiplyByCoefficient");
        var mutator = new StepMutator<Pipe<int, int, int, int>>("MultiplyByCoefficient", 1, pipe => async (input, next, ct) => await pipe(input + 5, next, ct));
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassThreeStepHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int, int, int>("TestThreeStepPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async (input, ct) => await handler(input + 1, ct));
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, string, int, string, int>("TestIfStepPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, int, string, int>>("TrimStringMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredIfStep<string, int, string, int, string, int, int, int>("TestIfStepPipeline", "CheckIntOrFloat");
        var mut = new StepMutator<IfSelector<string, int, string, int, int, int>>("CheckIntOrFloatMutator", 1, sel => async (input, ifNext, next, ct) => await ifNext(input, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfRoundToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, double, int, int, int>("FloatProcessing", "RoundToInt");
        var mut = new StepMutator<Pipe<double, int, int, int>>("RoundToIntMutator", 1, pipe => async (input, next, ct) =>
        {
            input += 1;
            var rounded = (int)Math.Round(input);
            return await next(rounded, ct);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfAddConstantMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, int, int, int, int>("TestIfStepPipeline", "AddConstant");
        var mut = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int, int, int>("TestIfStepPipeline", "TestHandler");
        var mut = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async (input, ct) => await handler(input, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfElseTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, string, int, string, int>("TestIfElseStepPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, int, string, int>>("TrimStringMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfElseSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredIfElseStep<string, int, string, int, string, int, int, int, int, int>("TestIfElseStepPipeline", "CheckIntOrFloat");
        var mut = new StepMutator<IfElseSelector<string, int, string, int, int, int>>("CheckIntOrFloatMutator", 1, sel => async (input, trueNext, falseNext, ct) =>
        {
            if (int.TryParse(input, out _))
            {
                return await trueNext(input, ct);
            }
            else
            {
                return await falseNext(0, ct);
            }
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfElseParseFloatMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, string, int, double, int>("FloatProcessing", "ParseFloat");
        var mut = new StepMutator<Pipe<string, int, double, int>>("ParseFloatMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfElseRoundToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, double, int, int, int>("FloatProcessing", "RoundToInt");
        var mut = new StepMutator<Pipe<double, int, int, int>>("RoundToIntMutator", 1, pipe => async (input, next, ct) =>
        {
            input += 1;
            var rounded = (int)Math.Round(input);
            return await next(rounded, ct);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfElseMultiplyMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("IntProcessing", "Multiply");
        var mut = new StepMutator<Pipe<int, int, int, int>>("IntProcessingMutator", 1, pipe => async (input, next, ct) => await pipe(input + 2, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfElseAddConstantMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, int, int, int, int>("TestIfElseStepPipeline", "AddConstant");
        var mut = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassIfElseHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int, int, int>("TestIfElseStepPipeline", "TestHandler");
        var mut = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async (input, ct) => await handler(input + 1, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassSwitchTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, string, int, string, int>("TestSwitchPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, int, string, int>>("TrimStringMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassSwitchSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredSwitchStep<string, int, string, int, int, int, int, int, int, int>("TestSwitchPipeline", "NumberRangeSwitch");
        var mut = new StepMutator<SwitchSelector<string, int, int, int, int, int>>("NumberRangeSwitchMutator", 1, selector => async (input, cases, defaultNext, ct) =>
        {
            if (int.TryParse(input, out var number))
            {
                if (number > 50) return await cases["GreaterThan100"](number, ct);
                if (number > 0) return await cases["BetweenZeroAndHundred"](number, ct);
                return await cases["LessThanZero"](number, ct);
            }
            return await defaultNext(input.Length, ct);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassSwitchMultiplyByThreeMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("MultiplyByThree", "MultiplyOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("MultiplyByThreeMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassSwitchAddTwoMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("AddTwo", "AddOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("AddTwoMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassSwitchMultiplyByTwoMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("MultiplyByTwo", "MultiplyOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("MultiplyByTwoMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassSwitchKeepZeroMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("KeepZero", "IdentityOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("KeepZeroMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassSwitchStringLengthMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("StringLengthPipeline", "IdentityOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("StringLengthMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassSwitchHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int, int, int>("TestSwitchPipeline", "TestHandler");
        var mut = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async (input, ct) => await handler(input + 3, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassDigitContentForkStep : IForkStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>
{
    public string Name => "DigitContentFork";
    public async ValueTask<(int?, string?)> Handle(string input, Handler<string, (int?, string?)> digitBranch, Handler<string, (int?, string?)> nonDigitBranch, CancellationToken ct = default)
    {
        var onlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
        return onlyDigits ? await digitBranch(input, ct) : await nonDigitBranch(input, ct);
    }
    public Pipeline<string, (int?, string?)> BuildBranchAPipeline(Space space)
    {
        return space.CreatePipeline<string, (int?, string?)>("DigitProcessing")
            .StartWithLinear<string, (int?, string?)>("RemoveNonDigits", async (input, next, ct) =>
            {
                var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                return await next(digitsOnly, ct);
            })
            .ThenLinear<int, (int?, string?)>("ParseToInt", async (input, next, ct) =>
            {
                if (int.TryParse(input, out var number)) return await next(number, ct);
                return await next(0, ct);
            })
            .HandleWith("IntHandler", async (input, ct) => await Task.FromResult<(int?, string?)>((input, null)))
            .BuildPipeline();
    }
    public Pipeline<string, (int?, string?)> BuildBranchBPipeline(Space space)
    {
        return space.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
            .StartWithLinear<string, (int?, string?)>("RemoveDigits", async (input, next, ct) =>
            {
                var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                return await next(nonDigitsOnly, ct);
            })
            .ThenLinear<string, (int?, string?)>("AddSpaces", async (input, next, ct) =>
            {
                var withSpaces = $"  {input}  ";
                return await next(withSpaces, ct);
            })
            .HandleWith("StringHandler", async (input, ct) => await Task.FromResult<(int?, string?)>((null, input)))
            .BuildPipeline();
    }
}

public class TestReturningCancellableClassForkStepPipeline
{
    private readonly Handler<string, (int?, string?)> _compiled;
    public TestReturningCancellableClassForkStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var fork = new TestReturningCancellableClassDigitContentForkStep();
        var space = new Space();
        var pipeline = space.CreatePipeline<string, (int?, string?)>("TestForkPipeline")
            .StartWithLinear<string, (int?, string?)>("TrimString", async (input, next, ct) => await next(input.Trim(), ct))
            .ThenFork(fork)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public ValueTask<(int?, string?)> Run(string input, CancellationToken ct) => _compiled(input, ct);
}

public class TestReturningCancellableClassForkTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("TestForkPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("TrimStringMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassForkSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredForkStep<string, (int?, string?), string, (int?, string?), string, (int?, string?), string, (int?, string?)>("TestForkPipeline", "DigitContentFork");
        var mut = new StepMutator<ForkSelector<string, (int?, string?), string, (int?, string?), string, (int?, string?)>>("DigitContentForkMutator", 1, selector => async (input, digitBranch, nonDigitBranch, ct) =>
        {
            if (input.Length > 3) return await digitBranch(input, ct);
            return await nonDigitBranch(input, ct);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassForkRemoveNonDigitsMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("DigitProcessing", "RemoveNonDigits");
        var mut = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("RemoveNonDigitsMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassForkParseToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), int, (int?, string?)>("DigitProcessing", "ParseToInt");
        var mut = new StepMutator<Pipe<string, (int?, string?), int, (int?, string?)>>("ParseToIntMutator", 1, pipe => async (input, next, ct) =>
        {
            if (int.TryParse(input, out var number)) return await next(number + 5, ct);
            return await next(0 + 5, ct);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassForkIntHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, (int?, string?), int, (int?, string?)>("DigitProcessing", "IntHandler");
        var mut = new StepMutator<Handler<int, (int?, string?)>>("IntHandlerMutator", 1, handler => async (input, ct) => await handler(input, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassForkRemoveDigitsMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "RemoveDigits");
        var mut = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("RemoveDigitsMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassForkAddSpacesMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "AddSpaces");
        var mut = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("AddSpacesMutator", 1, pipe => async (input, next, ct) => await next($"***{input}***", ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassForkStringHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "StringHandler");
        var mut = new StepMutator<Handler<string, (int?, string?)>>("StringHandlerMutator", 1, handler => async (input, ct) => await handler(input, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkStep : IMultiForkStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>
{
    public string Name => "ClassifyStringContent";
    public async ValueTask<(int?, string?, char[]?)> Handle(string input, IReadOnlyDictionary<string, Handler<string, (int?, string?, char[]?)>> branches, Handler<char[], (int?, string?, char[]?)> defaultNext, CancellationToken ct = default)
    {
        var onlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
        var onlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
        var onlySpecial = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        if (onlyDigits) return await branches["DigitBranch"](input, ct);
        if (onlyLetters) return await branches["LetterBranch"](input, ct);
        if (onlySpecial) return await branches["SpecialCharBranch"](input, ct);
        return await defaultNext(input.ToCharArray(), ct);
    }
    public IReadOnlyDictionary<string, Pipeline<string, (int?, string?, char[]?)>> BuildBranchesPipelines(Space space)
    {
        return new Dictionary<string, Pipeline<string, (int?, string?, char[]?)>>
        {
            ["DigitBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")!,
            ["LetterBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")!,
            ["SpecialCharBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")!
        }.AsReadOnly();
    }
    public Pipeline<char[], (int?, string?, char[]?)> BuildDefaultPipeline(Space space)
    {
        return space.GetPipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")!;
    }
}

public class TestReturningCancellableClassMultiForkStepPipeline
{
    private readonly Handler<string, (int?, string?, char[]?)> _compiled;
    public TestReturningCancellableClassMultiForkStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var space = new Space();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", async (input, next, ct) => await next(int.TryParse(input, out var n) ? n : 0, ct))
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", async (input, next, ct) => await next(input + 10, ct))
            .HandleWith("IntHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((input, null, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", async (input, next, ct) => await next($"  {input}  ", ct))
            .HandleWith("StringHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((null, input, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("RemoveWhitespace", async (input, next, ct) => await next(new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray()), ct))
            .ThenLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", async (input, next, ct) => await next(input.ToCharArray(), ct))
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", async (input, next, ct) => await next(input.Distinct().ToArray(), ct))
            .HandleWith("CharArrayHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input)))
            .BuildPipeline();

        space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount), (int?, string?, char[]?)>("CountDigitsAndLetters", async (input, next, ct) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                return await next((digitCount, letterCount), ct);
            })
            .ThenLinear<int, (int?, string?, char[]?)>("CalculateRatio", async (input, next, ct) =>
            {
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                return await next(ratio, ct);
            })
            .HandleWith("IntHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((input, null, null)))
            .BuildPipeline();

        var multiFork = new TestReturningCancellableClassMultiForkStep();
        var pipeline = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", async (input, next, ct) => await next(input.Trim(), ct))
            .ThenMultiFork(multiFork)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public ValueTask<(int?, string?, char[]?)> Run(string input, CancellationToken ct) => _compiled(input, ct);
}

public class TestReturningCancellableClassMultiForkTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("TestMultiForkPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("TrimStringMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredMultiForkStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("TestMultiForkPipeline", "ClassifyStringContent");
        var mut = new StepMutator<MultiForkSelector<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("ClassifyStringContentMutator", 1, selector => async (input, branches, defaultNext, ct) =>
        {
            var onlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
            if (onlyDigits) return await branches["DigitBranch"](input, ct);
            return await branches["SpecialCharBranch"](input, ct);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkParseToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "ParseStringToInt");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>>("ParseStringToIntMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkAddConstantMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), int?, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "AddConstant");
        var mut = new StepMutator<Pipe<int?, (int?, string?, char[]?), int?, (int?, string?, char[]?)>>("AddConstantMutator", 1, pipe => async (input, next, ct) => await next((input ?? 0) + 10 + 5, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkIntHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "IntHandler");
        var mut = new StepMutator<Handler<int?, (int?, string?, char[]?)>>("IntHandlerMutatorDigit", 1, handler => async (input, ct) => await handler(input, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkAddSpacesMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("LetterProcessingPipeline", "AddSpaces");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("AddSpacesMutator", 1, pipe => async (input, next, ct) => await next($"***{input}***", ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkStringHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("LetterProcessingPipeline", "StringHandler");
        var mut = new StepMutator<Handler<string, (int?, string?, char[]?)>>("StringHandlerMutator", 1, handler => async (input, ct) => await handler(input, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkRemoveWhitespaceMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "RemoveWhitespace");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("RemoveWhitespaceMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkConvertToCharArrayMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "ConvertToCharArray");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("ConvertToCharArrayMutator", 1, pipe => async (input, next, ct) => await next((input + "_").ToCharArray(), ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkRemoveDuplicatesMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), char[], (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "RemoveDuplicates");
        var mut = new StepMutator<Pipe<char[], (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("RemoveDuplicatesMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkCountDigitsAndLettersMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<char[], (int?, string?, char[]?), char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?)>("DefaultProcessingPipeline", "CountDigitsAndLetters");
        var mut = new StepMutator<Pipe<char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?)>>("CountDigitsAndLettersMutator", 1, pipe => async (input, next, ct) => await pipe(input, next, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkCalculateRatioMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?), int, (int?, string?, char[]?)>("DefaultProcessingPipeline", "CalculateRatio");
        var mut = new StepMutator<Pipe<(int DigitCount, int LetterCount), (int?, string?, char[]?), int, (int?, string?, char[]?)>>("CalculateRatioMutator", 1, pipe => async (input, next, ct) =>
        {
            var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
            return await next(ratio + 2, ct);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningCancellableClassMultiForkDefaultIntHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<char[], (int?, string?, char[]?), int, (int?, string?, char[]?)>("DefaultProcessingPipeline", "IntHandler");
        var mut = new StepMutator<Handler<int, (int?, string?, char[]?)>>("IntHandlerMutatorDefault", 1, handler => async (input, ct) => await handler(input, ct));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}


