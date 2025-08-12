using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.ReturningAsync;

namespace K1vs.DotNetPipe.Tests.ReturningAsync;

public class WithMutationClassReturningPipelineTests
{
    [Theory]
    [InlineData(-4, -3)]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStep_ShouldReturn(int value, int expected)
    {
        var test = new TestReturningClassPipeline([
            new TestReturningClassHandlerMutator()
        ]);
        var result = await test.Run(value);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-4, 3)]   // ((-4 * 2) + 10) + 1 = 3
    [InlineData(0, 11)]   // ((0 * 2) + 10) + 1 = 11
    [InlineData(2, 15)]   // ((2 * 2) + 10) + 1 = 15
    public async Task BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldReturn(int input, int expected)
    {
        var test = new TestReturningClassTwoStepPipeline([
            new TestReturningClassLinearMutator(),
            new TestReturningClassTwoStepHandlerMutator()
        ]);
        var result = await test.Run(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(2, 45)]
    [InlineData(0, 41)]
    [InlineData(-1, 39)]
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldReturn(int input, int expected)
    {
        var test = new TestReturningClassThreeStepPipeline([
            new TestReturningClassFirstLinearMutator(),
            new TestReturningClassSecondLinearMutator(),
            new TestReturningClassThreeStepHandlerMutator()
        ]);
        var result = await test.Run(input);
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
        var test = new TestReturningClassIfStepPipeline([
            new TestReturningClassIfTrimMutator(),
            new TestReturningClassIfSelectorMutator(),
            new TestReturningClassIfRoundToIntMutator(),
            new TestReturningClassIfAddConstantMutator(),
            new TestReturningClassIfHandlerMutator()
        ]);
        var result = await test.Run(inputValue);
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
        var test = new TestReturningClassIfElseStepPipeline([
            new TestReturningClassIfElseTrimMutator(),
            new TestReturningClassIfElseSelectorMutator(),
            new TestReturningClassIfElseParseFloatMutator(),
            new TestReturningClassIfElseRoundToIntMutator(),
            new TestReturningClassIfElseMultiplyMutator(),
            new TestReturningClassIfElseAddConstantMutator(),
            new TestReturningClassIfElseHandlerMutator()
        ]);
        var result = await test.Run(inputValue);
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
        var test = new TestReturningClassSwitchStepPipeline([
            new TestReturningClassSwitchTrimMutator(),
            new TestReturningClassSwitchSelectorMutator(),
            new TestReturningClassSwitchMultiplyByThreeMutator(),
            new TestReturningClassSwitchAddTwoMutator(),
            new TestReturningClassSwitchMultiplyByTwoMutator(),
            new TestReturningClassSwitchKeepZeroMutator(),
            new TestReturningClassSwitchStringLengthMutator(),
            new TestReturningClassSwitchHandlerMutator()
        ]);
        var result = await test.Run(inputValue);
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
        var test = new TestReturningClassForkStepPipeline([
            new TestReturningClassForkTrimMutator(),
            new TestReturningClassForkSelectorMutator(),
            new TestReturningClassForkRemoveNonDigitsMutator(),
            new TestReturningClassForkParseToIntMutator(),
            new TestReturningClassForkIntHandlerMutator(),
            new TestReturningClassForkRemoveDigitsMutator(),
            new TestReturningClassForkAddSpacesMutator(),
            new TestReturningClassForkStringHandlerMutator()
        ]);
        var result = await test.Run(inputValue);
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
        var test = new TestReturningClassMultiForkStepPipeline(new IMutator<Space>[]
        {
            new TestReturningClassMultiForkTrimMutator(),
            new TestReturningClassMultiForkSelectorMutator(),
            new TestReturningClassMultiForkParseToIntMutator(),
            new TestReturningClassMultiForkAddConstantMutator(),
            new TestReturningClassMultiForkIntHandlerMutator(),
            new TestReturningClassMultiForkAddSpacesMutator(),
            new TestReturningClassMultiForkStringHandlerMutator(),
            new TestReturningClassMultiForkRemoveWhitespaceMutator(),
            new TestReturningClassMultiForkConvertToCharArrayMutator(),
            new TestReturningClassMultiForkRemoveDuplicatesMutator(),
            new TestReturningClassMultiForkCountDigitsAndLettersMutator(),
            new TestReturningClassMultiForkCalculateRatioMutator(),
            new TestReturningClassMultiForkDefaultIntHandlerMutator()
        });

        var result = await test.Run(inputValue);
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

// Simple class-based steps for Returning
public class TestReturningClassHandlerStep : IHandlerStep<int, int>
{
    public string Name => "TestHandler";
    public Task<int> Handle(int input) => Task.FromResult(input);
}

public class TestReturningClassLinearStep : ILinearStep<int, int, int, int>
{
    private const int ConstantToAdd = 10;
    public string Name => "AddConstant";
    public async Task<int> Handle(int input, Handler<int, int> next)
    {
        var result = input + ConstantToAdd;
        return await next(result);
    }
}

public class TestReturningClassSecondLinearStep : ILinearStep<int, int, int, int>
{
    private const int MultiplierCoefficient = 2;
    public string Name => "MultiplyByCoefficient";
    public async Task<int> Handle(int input, Handler<int, int> next)
    {
        var result = input * MultiplierCoefficient;
        return await next(result);
    }
}

// If step: steps
public class TestReturningClassTrimStep : ILinearStep<string, int, string, int>
{
    public string Name => "TrimString";
    public async Task<int> Handle(string input, Handler<string, int> next)
    {
        return await next(input.Trim());
    }
}

public class TestReturningClassIfStep : IIfStep<string, int, string, int, int, int>
{
    public string Name => "CheckIntOrFloat";
    public async Task<int> Handle(string input, Handler<string, int> ifNext, Handler<int, int> next)
    {
        if (int.TryParse(input, out var intValue))
        {
            return await next(intValue);
        }
        return await ifNext(input);
    }
    public OpenPipeline<string, int, int, int> BuildTruePipeline(Space space)
    {
        return space.CreatePipeline<string, int>("FloatProcessing")
            .StartWithLinear<double, int>("ParseFloat", async (val, nxt) =>
            {
                if (double.TryParse(val, out var f)) return await nxt(f);
                return 0;
            })
            .ThenLinear<int, int>("RoundToInt", async (val, nxt) =>
            {
                var rounded = (int)Math.Round(val);
                return await nxt(rounded);
            })
            .BuildOpenPipeline();
    }
}

public class TestReturningClassIfAddConstantStep : ILinearStep<int, int, int, int>
{
    public string Name => "AddConstant";
    public async Task<int> Handle(int input, Handler<int, int> next)
    {
        var result = input + 2;
        return await next(result);
    }
}

public class TestReturningClassIfHandlerStep : IHandlerStep<int, int>
{
    public string Name => "TestHandler";
    public Task<int> Handle(int input) => Task.FromResult(input);
}

// IfElse step: steps
public class TestReturningClassIfElseStep : IIfElseStep<string, int, string, int, int, int, int, int>
{
    public string Name => "CheckIntOrFloat";
    public async Task<int> Handle(string input, Handler<string, int> ifNext, Handler<int, int> elseNext)
    {
        if (int.TryParse(input, out var intValue))
        {
            return await elseNext(intValue);
        }
        return await ifNext(input);
    }
    public OpenPipeline<string, int, int, int> BuildTruePipeline(Space space)
    {
        return space.CreatePipeline<string, int>("FloatProcessing")
            .StartWithLinear<double, int>("ParseFloat", async (val, nxt) =>
            {
                if (double.TryParse(val, out var f)) return await nxt(f);
                return 0;
            })
            .ThenLinear<int, int>("RoundToInt", async (val, nxt) =>
            {
                var rounded = (int)Math.Round(val);
                return await nxt(rounded);
            })
            .BuildOpenPipeline();
    }
    public OpenPipeline<int, int, int, int> BuildFalsePipeline(Space space)
    {
        return space.CreatePipeline<int, int>("IntProcessing")
            .StartWithLinear<int, int>("Multiply", async (val, nxt) =>
            {
                var result = val * 2;
                return await nxt(result);
            })
            .BuildOpenPipeline();
    }
}

public class TestReturningClassIfElseAddConstantStep : ILinearStep<int, int, int, int>
{
    public string Name => "AddConstant";
    public async Task<int> Handle(int input, Handler<int, int> next)
    {
        var result = input + 3;
        return await next(result);
    }
}

public class TestReturningClassIfElseHandlerStep : IHandlerStep<int, int>
{
    public string Name => "TestHandler";
    public Task<int> Handle(int input) => Task.FromResult(input);
}

// Switch step: steps
public class TestReturningClassNumberRangeSwitchStep : ISwitchStep<string, int, int, int, int, int, int, int>
{
    public string Name => "NumberRangeSwitch";
    public async Task<int> Handle(string input,
        IReadOnlyDictionary<string, Handler<int, int>> cases,
        Handler<int, int> defaultNext)
    {
        if (int.TryParse(input, out var number))
        {
            if (number > 100) return await cases["GreaterThan100"](number);
            if (number > 0) return await cases["BetweenZeroAndHundred"](number);
            if (number < 0) return await cases["LessThanZero"](number);
            return await cases["EqualToZero"](number);
        }
        return await defaultNext(input.Length);
    }

    public IReadOnlyDictionary<string, OpenPipeline<int, int, int, int>> BuildCasesPipelines(Space space)
    {
        return new Dictionary<string, OpenPipeline<int, int, int, int>>
        {
            ["GreaterThan100"] = space.CreatePipeline<int, int>("MultiplyByThree")
                .StartWithLinear<int, int>("MultiplyOperation", async (input, next) => await next(input * 3))
                .BuildOpenPipeline(),
            ["BetweenZeroAndHundred"] = space.CreatePipeline<int, int>("AddTwo")
                .StartWithLinear<int, int>("AddOperation", async (input, next) => await next(input + 2))
                .BuildOpenPipeline(),
            ["LessThanZero"] = space.CreatePipeline<int, int>("MultiplyByTwo")
                .StartWithLinear<int, int>("MultiplyOperation", async (input, next) => await next(input * 2))
                .BuildOpenPipeline(),
            ["EqualToZero"] = space.CreatePipeline<int, int>("KeepZero")
                .StartWithLinear<int, int>("IdentityOperation", async (input, next) => await next(input))
                .BuildOpenPipeline()
        }.AsReadOnly();
    }

    public OpenPipeline<int, int, int, int> BuildDefaultPipeline(Space space)
    {
        return space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", async (input, next) => await next(input))
            .BuildOpenPipeline();
    }
}

// Pipelines
public class TestReturningClassPipeline
{
    private readonly Handler<int, int> _compiled;

    public TestReturningClassPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var handler = new TestReturningClassHandlerStep();
        var pipeline = Pipelines.CreateReturningAsyncPipeline<int, int>("TestPipeline")
            .StartWithHandler(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }

    public Task<int> Run(int input) => _compiled(input);
}

public class TestReturningClassTwoStepPipeline
{
    private readonly Handler<int, int> _compiled;

    public TestReturningClassTwoStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var linear = new TestReturningClassLinearStep();
        var handler = new TestReturningClassHandlerStep();
        var pipeline = Pipelines.CreateReturningAsyncPipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear(linear)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }

    public Task<int> Run(int input) => _compiled(input);
}

public class TestReturningClassThreeStepPipeline
{
    private readonly Handler<int, int> _compiled;

    public TestReturningClassThreeStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var first = new TestReturningClassLinearStep();
        var second = new TestReturningClassSecondLinearStep();
        var handler = new TestReturningClassHandlerStep();
        var pipeline = Pipelines.CreateReturningAsyncPipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear(first)
            .ThenLinear(second)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }

    public Task<int> Run(int input) => _compiled(input);
}

public class TestReturningClassIfStepPipeline
{
    private readonly Handler<string, int> _compiled;
    public TestReturningClassIfStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var trim = new TestReturningClassTrimStep();
        var ifStep = new TestReturningClassIfStep();
        var add = new TestReturningClassIfAddConstantStep();
        var handler = new TestReturningClassIfHandlerStep();
        var pipeline = Pipelines.CreateReturningAsyncPipeline<string, int>("TestIfStepPipeline")
            .StartWithLinear(trim)
            .ThenIf(ifStep)
            .ThenLinear(add)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public Task<int> Run(string input) => _compiled(input);
}

public class TestReturningClassIfElseStepPipeline
{
    private readonly Handler<string, int> _compiled;
    public TestReturningClassIfElseStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var trim = new TestReturningClassTrimStep();
        var ifElse = new TestReturningClassIfElseStep();
        var add = new TestReturningClassIfElseAddConstantStep();
        var handler = new TestReturningClassIfElseHandlerStep();
        var pipeline = Pipelines.CreateReturningAsyncPipeline<string, int>("TestIfElseStepPipeline")
            .StartWithLinear(trim)
            .ThenIfElse(ifElse)
            .ThenLinear(add)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public Task<int> Run(string input) => _compiled(input);
}

public class TestReturningClassSwitchStepPipeline
{
    private readonly Handler<string, int> _compiled;
    public TestReturningClassSwitchStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var space = Pipelines.CreateReturningAsyncSpace();
        var trim = new TestReturningClassTrimStep();
        var switchStep = new TestReturningClassNumberRangeSwitchStep();
        var handler = new TestReturningClassIfHandlerStep();

        var pipeline = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear(trim)
            .ThenSwitch(switchStep)
            .HandleWith(handler)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public Task<int> Run(string input) => _compiled(input);
}

// If step: mutators
public class TestReturningClassIfTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, string, int, string, int>("TestIfStepPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, int, string, int>>("TrimStringMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredIfStep<string, int, string, int, string, int, int, int>("TestIfStepPipeline", "CheckIntOrFloat");
        var mut = new StepMutator<IfSelector<string, int, string, int, int, int>>("CheckIntOrFloatMutator", 1, sel => async (input, ifNext, next) => await ifNext(input));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfRoundToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, double, int, int, int>("FloatProcessing", "RoundToInt");
        var mut = new StepMutator<Pipe<double, int, int, int>>("RoundToIntMutator", 1, pipe => async (input, next) =>
        {
            input += 1;
            var rounded = (int)Math.Round(input);
            return await next(rounded);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfAddConstantMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, int, int, int, int>("TestIfStepPipeline", "AddConstant");
        var mut = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int, int, int>("TestIfStepPipeline", "TestHandler");
        var mut = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async input => await handler(input));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

// IfElse step: mutators
public class TestReturningClassIfElseTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, string, int, string, int>("TestIfElseStepPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, int, string, int>>("TrimStringMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfElseSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredIfElseStep<string, int, string, int, string, int, int, int, int, int>("TestIfElseStepPipeline", "CheckIntOrFloat");
        var mut = new StepMutator<IfElseSelector<string, int, string, int, int, int>>("CheckIntOrFloatMutator", 1, sel => async (input, trueNext, falseNext) =>
        {
            if (int.TryParse(input, out _))
            {
                return await trueNext(input);
            }
            else
            {
                return await falseNext(0);
            }
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfElseParseFloatMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, string, int, double, int>("FloatProcessing", "ParseFloat");
        var mut = new StepMutator<Pipe<string, int, double, int>>("ParseFloatMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfElseRoundToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, double, int, int, int>("FloatProcessing", "RoundToInt");
        var mut = new StepMutator<Pipe<double, int, int, int>>("RoundToIntMutator", 1, pipe => async (input, next) =>
        {
            input += 1;
            var rounded = (int)Math.Round(input);
            return await next(rounded);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfElseMultiplyMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("IntProcessing", "Multiply");
        var mut = new StepMutator<Pipe<int, int, int, int>>("IntProcessingMutator", 1, pipe => async (input, next) =>
        {
            input += 2;
            return await pipe(input, next);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfElseAddConstantMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, int, int, int, int>("TestIfElseStepPipeline", "AddConstant");
        var mut = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassIfElseHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int, int, int>("TestIfElseStepPipeline", "TestHandler");
        var mut = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async input =>
        {
            input += 1;
            return await handler(input);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassSwitchTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, string, int, string, int>("TestSwitchPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, int, string, int>>("TrimStringMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassSwitchSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredSwitchStep<string, int, string, int, int, int, int, int, int, int>("TestSwitchPipeline", "NumberRangeSwitch");
        var mut = new StepMutator<SwitchSelector<string, int, int, int, int, int>>("NumberRangeSwitchMutator", 1, selector => async (input, cases, defaultNext) =>
        {
            if (int.TryParse(input, out var number))
            {
                if (number > 50) return await cases["GreaterThan100"](number);
                if (number > 0) return await cases["BetweenZeroAndHundred"](number);
                return await cases["LessThanZero"](number);
            }
            return await defaultNext(input.Length);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassSwitchMultiplyByThreeMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("MultiplyByThree", "MultiplyOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("MultiplyByThreeMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassSwitchAddTwoMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("AddTwo", "AddOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("AddTwoMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassSwitchMultiplyByTwoMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("MultiplyByTwo", "MultiplyOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("MultiplyByTwoMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassSwitchKeepZeroMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("KeepZero", "IdentityOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("KeepZeroMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassSwitchStringLengthMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("StringLengthPipeline", "IdentityOperation");
        var mut = new StepMutator<Pipe<int, int, int, int>>("StringLengthMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassSwitchHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int, int, int>("TestSwitchPipeline", "TestHandler");
        var mut = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async input =>
        {
            input += 3;
            return await handler(input);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

// Fork step: steps
public class TestReturningClassDigitContentForkStep : IForkStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>
{
    public string Name => "DigitContentFork";
    public async Task<(int?, string?)> Handle(string input, Handler<string, (int?, string?)> digitBranch, Handler<string, (int?, string?)> nonDigitBranch)
    {
        var onlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
        return onlyDigits ? await digitBranch(input) : await nonDigitBranch(input);
    }
    public Pipeline<string, (int?, string?)> BuildBranchAPipeline(Space space)
    {
        return space.CreatePipeline<string, (int?, string?)>("DigitProcessing")
            .StartWithLinear<string, (int?, string?)>("RemoveNonDigits", async (input, next) =>
            {
                var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                return await next(digitsOnly);
            })
            .ThenLinear<int, (int?, string?)>("ParseToInt", async (input, next) =>
            {
                if (int.TryParse(input, out var number)) return await next(number);
                return await next(0);
            })
            .HandleWith("IntHandler", async (input) => await Task.FromResult<(int?, string?)>((input, null)))
            .BuildPipeline();
    }
    public Pipeline<string, (int?, string?)> BuildBranchBPipeline(Space space)
    {
        return space.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
            .StartWithLinear<string, (int?, string?)>("RemoveDigits", async (input, next) =>
            {
                var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                return await next(nonDigitsOnly);
            })
            .ThenLinear<string, (int?, string?)>("AddSpaces", async (input, next) =>
            {
                var withSpaces = $"  {input}  ";
                return await next(withSpaces);
            })
            .HandleWith("StringHandler", async (input) => await Task.FromResult<(int?, string?)>((null, input)))
            .BuildPipeline();
    }
}

public class TestReturningClassForkStepPipeline
{
    private readonly Handler<string, (int?, string?)> _compiled;
    public TestReturningClassForkStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var trim = new TestReturningClassTrimStep(); // returns int; adapt with lambda linear
        var fork = new TestReturningClassDigitContentForkStep();
        var space = Pipelines.CreateReturningAsyncSpace();
        var pipeline = space.CreatePipeline<string, (int?, string?)>("TestForkPipeline")
            .StartWithLinear<string, (int?, string?)>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed);
            })
            .ThenFork(fork)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public Task<(int?, string?)> Run(string input) => _compiled(input);
}

// Fork step: mutators
public class TestReturningClassForkTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("TestForkPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("TrimStringMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassForkSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredForkStep<string, (int?, string?), string, (int?, string?), string, (int?, string?), string, (int?, string?)>("TestForkPipeline", "DigitContentFork");
        var mut = new StepMutator<ForkSelector<string, (int?, string?), string, (int?, string?), string, (int?, string?)>>("DigitContentForkMutator", 1, selector => async (input, digitBranch, nonDigitBranch) =>
        {
            if (input.Length > 3) return await digitBranch(input);
            return await nonDigitBranch(input);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassForkRemoveNonDigitsMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("DigitProcessing", "RemoveNonDigits");
        var mut = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("RemoveNonDigitsMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassForkParseToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), int, (int?, string?)>("DigitProcessing", "ParseToInt");
        var mut = new StepMutator<Pipe<string, (int?, string?), int, (int?, string?)>>("ParseToIntMutator", 1, pipe => async (input, next) =>
        {
            if (int.TryParse(input, out var number)) return await next(number + 5);
            return await next(0 + 5);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassForkIntHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, (int?, string?), int, (int?, string?)>("DigitProcessing", "IntHandler");
        var mut = new StepMutator<Handler<int, (int?, string?)>>("IntHandlerMutator", 1, handler => async input => await handler(input));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassForkRemoveDigitsMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "RemoveDigits");
        var mut = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("RemoveDigitsMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassForkAddSpacesMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "AddSpaces");
        var mut = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("AddSpacesMutator", 1, pipe => async (input, next) =>
        {
            var withAsterisks = $"***{input}***";
            return await next(withAsterisks);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassForkStringHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "StringHandler");
        var mut = new StepMutator<Handler<string, (int?, string?)>>("StringHandlerMutator", 1, handler => async input => await handler(input));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

// MultiFork: steps
public class TestReturningClassMultiForkStep : IMultiForkStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>
{
    public string Name => "ClassifyStringContent";
    public async Task<(int?, string?, char[]?)> Handle(string input,
        IReadOnlyDictionary<string, Handler<string, (int?, string?, char[]?)>> branches,
        Handler<char[], (int?, string?, char[]?)> defaultNext)
    {
        var onlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
        var onlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
        var onlySpecial = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        if (onlyDigits) return await branches["DigitBranch"](input);
        if (onlyLetters) return await branches["LetterBranch"](input);
        if (onlySpecial) return await branches["SpecialCharBranch"](input);
        return await defaultNext(input.ToCharArray());
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

public class TestReturningClassMultiForkStepPipeline
{
    private readonly Handler<string, (int?, string?, char[]?)> _compiled;
    public TestReturningClassMultiForkStepPipeline(IEnumerable<IMutator<Space>> mutators)
    {
        var space = Pipelines.CreateReturningAsyncSpace();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", async (input, next) => await next(int.TryParse(input, out var n) ? n : 0))
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", async (input, next) => await next(input + 10))
            .HandleWith("IntHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((input, null, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", async (input, next) => await next($"  {input}  "))
            .HandleWith("StringHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((null, input, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("RemoveWhitespace", async (input, next) => await next(new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray())))
            .ThenLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", async (input, next) => await next(input.ToCharArray()))
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", async (input, next) => await next(input.Distinct().ToArray()))
            .HandleWith("CharArrayHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input)))
            .BuildPipeline();

        space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount), (int?, string?, char[]?)>("CountDigitsAndLetters", async (input, next) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                return await next((digitCount, letterCount));
            })
            .ThenLinear<int, (int?, string?, char[]?)>("CalculateRatio", async (input, next) =>
            {
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                return await next(ratio);
            })
            .HandleWith("IntHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((input, null, null)))
            .BuildPipeline();

        var trim = new TestReturningClassTrimStep();
        var multiFork = new TestReturningClassMultiForkStep();
        var pipeline = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", async (input, next) => await next(input.Trim()))
            .ThenMultiFork(multiFork)
            .BuildPipeline();
        _compiled = pipeline.Compile(cfg => cfg.Configure(mutators));
    }
    public Task<(int?, string?, char[]?)> Run(string input) => _compiled(input);
}

// MultiFork: mutators (returning)
public class TestReturningClassMultiForkTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("TestMultiForkPipeline", "TrimString");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("TrimStringMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkSelectorMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredMultiForkStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("TestMultiForkPipeline", "ClassifyStringContent");
        var mut = new StepMutator<MultiForkSelector<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("ClassifyStringContentMutator", 1, selector => async (input, branches, defaultNext) =>
        {
            var onlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
            if (onlyDigits) return await branches["DigitBranch"](input);
            return await branches["SpecialCharBranch"](input);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkParseToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "ParseStringToInt");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>>("ParseStringToIntMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkAddConstantMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), int?, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "AddConstant");
        var mut = new StepMutator<Pipe<int?, (int?, string?, char[]?), int?, (int?, string?, char[]?)>>("AddConstantMutator", 1, pipe => async (input, next) =>
        {
            var result = (input ?? 0) + 10 + 5;
            return await next(result);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkIntHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "IntHandler");
        var mut = new StepMutator<Handler<int?, (int?, string?, char[]?)>>("IntHandlerMutatorDigit", 1, handler => async input => await handler(input));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkAddSpacesMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("LetterProcessingPipeline", "AddSpaces");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("AddSpacesMutator", 1, pipe => async (input, next) =>
        {
            var withAsterisks = $"***{input}***";
            return await next(withAsterisks);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkStringHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("LetterProcessingPipeline", "StringHandler");
        var mut = new StepMutator<Handler<string, (int?, string?, char[]?)>>("StringHandlerMutator", 1, handler => async input => await handler(input));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkRemoveWhitespaceMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "RemoveWhitespace");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("RemoveWhitespaceMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkConvertToCharArrayMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "ConvertToCharArray");
        var mut = new StepMutator<Pipe<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("ConvertToCharArrayMutator", 1, pipe => async (input, next) =>
        {
            var inputWithUnderscore = input + "_";
            return await next(inputWithUnderscore.ToCharArray());
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkRemoveDuplicatesMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, (int?, string?, char[]?), char[], (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "RemoveDuplicates");
        var mut = new StepMutator<Pipe<char[], (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("RemoveDuplicatesMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkCountDigitsAndLettersMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<char[], (int?, string?, char[]?), char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?)>("DefaultProcessingPipeline", "CountDigitsAndLetters");
        var mut = new StepMutator<Pipe<char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?)>>("CountDigitsAndLettersMutator", 1, pipe => async (input, next) => await pipe(input, next));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkCalculateRatioMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?), int, (int?, string?, char[]?)>("DefaultProcessingPipeline", "CalculateRatio");
        var mut = new StepMutator<Pipe<(int DigitCount, int LetterCount), (int?, string?, char[]?), int, (int?, string?, char[]?)>>("CalculateRatioMutator", 1, pipe => async (input, next) =>
        {
            var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
            return await next(ratio + 2);
        });
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

public class TestReturningClassMultiForkDefaultIntHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<char[], (int?, string?, char[]?), int, (int?, string?, char[]?)>("DefaultProcessingPipeline", "IntHandler");
        var mut = new StepMutator<Handler<int, (int?, string?, char[]?)>>("IntHandlerMutatorDefault", 1, handler => async input => await handler(input));
        step.Mutators.AddMutator(mut, AddingMode.ExactPlace);
    }
}

// Mutators
public class TestReturningClassHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int, int, int>("TestPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async input =>
        {
            input += 1;
            return await handler(input);
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningClassTwoStepHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int, int, int>("TestTwoStepPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async input =>
        {
            input += 1;
            return await handler(input);
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningClassLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestTwoStepPipeline", "AddConstant");
        var mutator = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => async (input, next) =>
        {
            input *= 2;
            return await pipe(input, next);
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningClassFirstLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestThreeStepPipeline", "AddConstant");
        var mutator = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => async (input, next) =>
        {
            input += 5;
            return await pipe(input, next);
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningClassSecondLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestThreeStepPipeline", "MultiplyByCoefficient");
        var mutator = new StepMutator<Pipe<int, int, int, int>>("MultiplyByCoefficient", 1, pipe => async (input, next) =>
        {
            input += 5;
            return await pipe(input, next);
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

public class TestReturningClassThreeStepHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int, int, int>("TestThreeStepPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => async input =>
        {
            input += 1;
            return await handler(input);
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}



