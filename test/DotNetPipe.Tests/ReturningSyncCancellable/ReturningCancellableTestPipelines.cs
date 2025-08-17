using K1vs.DotNetPipe.ReturningSyncCancellable;

namespace K1vs.DotNetPipe.Tests.ReturningSyncCancellable;

public class TestReturningCancellablePipeline
{
    private readonly Pipeline<int, int> _pipeline;

    public TestReturningCancellablePipeline(Handler<int, int> handler)
    {
        _pipeline = Pipelines.CreateReturningSyncCancellablePipeline<int, int>("TestPipeline")
            .StartWithHandler("TestHandler", handler)
            .BuildPipeline();
    }

    public int Run(int input, CancellationToken ct)
    {
        var compiled = _pipeline.Compile();
        return compiled(input, ct);
    }
}

public class TestReturningCancellableTwoStepPipeline
{
    private readonly Space _space;
    private readonly Handler<int, int> _handler;

    public TestReturningCancellableTwoStepPipeline(Handler<int, int> handler)
    {
        _space = Pipelines.CreateReturningSyncCancellableSpace();
        _handler = handler;
    }

    public int Run(int input, int constantToAdd, CancellationToken ct)
    {
        var pipeline = _space.CreatePipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (val, next, ct2) => next(val + constantToAdd, ct2))
            .HandleWith("TestHandler", _handler)
            .BuildPipeline().Compile();
        return pipeline(input, ct);
    }
}

public class TestReturningCancellableThreeStepPipeline
{
    private readonly Space _space;
    private readonly Handler<int, int> _handler;

    public TestReturningCancellableThreeStepPipeline(Handler<int, int> handler)
    {
        _space = Pipelines.CreateReturningSyncCancellableSpace();
        _handler = handler;
    }

    public int Run(int input, int constantToAdd, int multiplier, CancellationToken ct)
    {
        var pipeline = _space.CreatePipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (val, next, ct2) => next(val + constantToAdd, ct2))
            .ThenLinear<int, int>("MultiplyByCoefficient", (val, next, ct2) => next(val * multiplier, ct2))
            .HandleWith("TestHandler", _handler)
            .BuildPipeline().Compile();
        return pipeline(input, ct);
    }
}

public class TestReturningCancellableIfStepPipeline
{
    private readonly Func<string, int, CancellationToken, int> _pipeline;

    public TestReturningCancellableIfStepPipeline()
    {
        _pipeline = (input, constantToAdd, ct) =>
        {
            var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<string, int>("TestIfStepPipeline")
                .StartWithLinear<string, int>("TrimString", (val, next, ct2) => next(val.Trim(), ct2))
                .ThenIf<string, int, int, int>("CheckIntOrFloat", (val, conditionalNext, next, ct2) =>
                {
                    if (int.TryParse(val, out var intValue))
                    {
                        return next(intValue, ct2);
                    }
                    else
                    {
                        return conditionalNext(val, ct2);
                    }
                }, s => s.CreatePipeline<string, int>("FloatProcessing")
                    .StartWithLinear<double, int>("ParseFloat", (val2, next, ct3) =>
                    {
                        if (double.TryParse(val2, out var floatValue))
                        {
                            return next(floatValue, ct3);
                        }
                        return 0;
                    })
                    .ThenLinear<int, int>("RoundToInt", (val3, next, ct3) =>
                    {
                        var rounded = (int)Math.Round(val3);
                        return next(rounded, ct3);
                    })
                    .BuildOpenPipeline())
                .ThenLinear<int, int>("AddConstant", (val, next, ct2) => next(val + constantToAdd, ct2))
                .HandleWith("TestHandler", (val, ct2) => val)
                .BuildPipeline().Compile();
            return pipeline(input, ct);
        };
    }

    public int Run(string input, int constantToAdd, CancellationToken ct)
    {
        return _pipeline(input, constantToAdd, ct);
    }
}

public class TestReturningCancellableIfElseStepPipeline
{
    private readonly Func<string, int, int, CancellationToken, int> _pipeline;

    public TestReturningCancellableIfElseStepPipeline()
    {
        _pipeline = (input, constantToAdd, multiplier, ct) =>
        {
            var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<string, int>("TestIfElseStepPipeline")
                .StartWithLinear<string, int>("TrimString", (val, next, ct2) => next(val.Trim(), ct2))
                .ThenIfElse<string, int, int, int, int, int>("CheckIntOrFloat", (val, trueNext, falseNext, ct2) =>
                {
                    if (int.TryParse(val, out var intValue))
                    {
                        return falseNext(intValue, ct2);
                    }
                    else
                    {
                        return trueNext(val, ct2);
                    }
                },
                s => s.CreatePipeline<string, int>("FloatProcessing")
                    .StartWithLinear<double, int>("ParseFloat", (val2, next, ct3) =>
                    {
                        if (double.TryParse(val2, out var floatValue))
                        {
                            return next(floatValue, ct3);
                        }
                        return 0;
                    })
                    .ThenLinear<int, int>("RoundToInt", (val3, next, ct3) =>
                    {
                        var rounded = (int)Math.Round(val3);
                        return next(rounded, ct3);
                    })
                    .BuildOpenPipeline(),
                s => s.CreatePipeline<int, int>("IntOrDefaultProcessing")
                    .StartWithLinear<int, int>("ParseIntOrDefault", (val2, next, ct3) => next(val2 * multiplier, ct3))
                    .BuildOpenPipeline())
                .ThenLinear<int, int>("AddConstant", (val, next, ct2) => next(val + constantToAdd, ct2))
                .HandleWith("TestHandler", (val, ct2) => val)
                .BuildPipeline().Compile();
            return pipeline(input, ct);
        };
    }

    public int Run(string input, int constantToAdd, int multiplier, CancellationToken ct)
    {
        return _pipeline(input, constantToAdd, multiplier, ct);
    }
}

public class TestReturningCancellableSwitchStepPipeline
{
    private readonly Func<string, CancellationToken, int> _pipeline;

    public TestReturningCancellableSwitchStepPipeline()
    {
        var space = Pipelines.CreateReturningSyncCancellableSpace();
        var defaultPipeline = space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", (input, next, ct) => next(input, ct))
            .BuildOpenPipeline();

        var compiled = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear<string, int>("TrimString", (input, next, ct) => next(input.Trim(), ct))
            .ThenSwitch<int, int, int, int, int, int>("NumberRangeSwitch", (input, cases, defaultNext, ct) =>
            {
                if (int.TryParse(input, out var number))
                {
                    if (number > 100) return cases["GreaterThan100"](number, ct);
                    if (number > 0) return cases["BetweenZeroAndHundred"](number, ct);
                    if (number < 0) return cases["LessThanZero"](number, ct);
                    return cases["EqualToZero"](number, ct);
                }
                return defaultNext(input.Length, ct);
            },
            s => new Dictionary<string, OpenPipeline<int, int, int, int>>
            {
                ["GreaterThan100"] = s.CreatePipeline<int, int>("MultiplyByThree")
                    .StartWithLinear<int, int>("MultiplyOperation", (input, next, ct) => next(input * 3, ct))
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = s.CreatePipeline<int, int>("AddTwo")
                    .StartWithLinear<int, int>("AddOperation", (input, next, ct) => next(input + 2, ct))
                    .BuildOpenPipeline(),
                ["LessThanZero"] = s.CreatePipeline<int, int>("MultiplyByTwo")
                    .StartWithLinear<int, int>("MultiplyOperation", (input, next, ct) => next(input * 2, ct))
                    .BuildOpenPipeline(),
                ["EqualToZero"] = s.CreatePipeline<int, int>("KeepZero")
                    .StartWithLinear<int, int>("IdentityOperation", (input, next, ct) => next(input, ct))
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", (input, ct) => input)
            .BuildPipeline().Compile();
        
        _pipeline = (input, ct) => compiled(input, ct);
    }

    public int Run(string input, CancellationToken ct)
    {
        return _pipeline(input, ct);
    }
}

public class TestReturningCancellableForkStepPipeline
{
    private readonly Func<string, CancellationToken, (int?, string?)> _pipeline;

    public TestReturningCancellableForkStepPipeline()
    {
        var compiled = Pipelines.CreateReturningSyncCancellablePipeline<string, (int?, string?)>("TestForkPipeline")
            .StartWithLinear<string, (int?, string?)>("TrimString", (input, next, ct) => next(input.Trim(), ct))
            .ThenFork<string, (int?, string?), string, (int?, string?)>("DigitContentFork", (input, digitBranch, nonDigitBranch, ct) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                if (containsOnlyDigits)
                {
                    return digitBranch(input, ct);
                }
                else
                {
                    return nonDigitBranch(input, ct);
                }
            },
            s => s.CreatePipeline<string, (int?, string?)>("DigitProcessing")
                .StartWithHandler("IntHandler", (input, ct) => (int.Parse(input), null))
                .BuildPipeline(),
            s => s.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
                .StartWithLinear<string, (int?, string?)>("RemoveDigits", (input, next, ct) =>
                {
                    var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                    return next(nonDigitsOnly, ct);
                })
                .ThenLinear<string, (int?, string?)>("AddSpaces", (input, next, ct) =>
                {
                    var withSpaces = $"  {input}  ";
                    return next(withSpaces, ct);
                })
                .HandleWith("StringHandler", (input, ct) => (null, input))
                .BuildPipeline())
            .BuildPipeline().Compile();
        
        _pipeline = (input, ct) => compiled(input, ct);
    }

    public (int?, string?) Run(string input, CancellationToken ct)
    {
        return _pipeline(input, ct);
    }
}

public class TestReturningCancellableMultiForkStepPipeline
{
    private readonly Func<string, CancellationToken, (int?, string?, char[]?)> _pipeline;

    public TestReturningCancellableMultiForkStepPipeline()
    {
        var space = Pipelines.CreateReturningSyncCancellableSpace();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", (input, next, ct) => next(int.TryParse(input, out var number) ? number : 0, ct))
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", (input, next, ct) => next(input + 10, ct))
            .HandleWith("IntHandler", (input, ct) => (input, null, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", (input, next, ct) => next($"  {input}  ", ct))
            .HandleWith("StringHandler", (input, ct) => (null, input, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", (input, next, ct) => next(input.ToCharArray(), ct))
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", (input, next, ct) => next(input.Distinct().ToArray(), ct))
            .HandleWith("CharArrayHandler", (input, ct) => (null, null, input))
            .BuildPipeline();
        
        var defaultPipeline = space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithHandler("DefaultHandler", (input, ct) => (null, null, input.Distinct().ToArray()))
            .BuildPipeline();

        var compiled = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", (input, next, ct) => next(input.Trim(), ct))
            .ThenMultiFork<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("ClassifyStringContent", (input, branches, defaultNext, ct) =>
            {
                if (!string.IsNullOrEmpty(input) && input.All(char.IsDigit)) return branches["DigitBranch"](input, ct);
                if (!string.IsNullOrEmpty(input) && input.All(char.IsLetter)) return branches["LetterBranch"](input, ct);
                if (!string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c))) return branches["SpecialCharBranch"](input, ct);
                return defaultNext(input.ToCharArray(), ct);
            },
            s => new Dictionary<string, Pipeline<string, (int?, string?, char[]?)>>
            {
                ["DigitBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")!,
                ["LetterBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            s => s.GetPipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")!)
            .BuildPipeline().Compile();
        
        _pipeline = (input, ct) => compiled(input, ct);
    }

    public (int?, string?, char[]?) Run(string input, CancellationToken ct)
    {
        return _pipeline(input, ct);
    }
}



