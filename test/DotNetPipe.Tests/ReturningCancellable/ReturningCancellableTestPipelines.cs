using K1vs.DotNetPipe.ReturningCancellable;

namespace K1vs.DotNetPipe.Tests.ReturningCancellable;

public class TestReturningCancellablePipeline
{
    private readonly Pipeline<int, int> _pipeline;

    public TestReturningCancellablePipeline(Handler<int, int> handler)
    {
        var space = new Space();
        _pipeline = space.CreatePipeline<int, int>("TestPipeline")
            .StartWithHandler("TestHandler", handler)
            .BuildPipeline();
    }

    public async ValueTask<int> Run(int input, CancellationToken ct)
    {
        var compiled = _pipeline.Compile();
        return await compiled(input, ct);
    }
}

public class TestReturningCancellableTwoStepPipeline
{
    private readonly Space _space;
    private readonly Handler<int, int> _handler;

    public TestReturningCancellableTwoStepPipeline(Handler<int, int> handler)
    {
        _space = new Space();
        _handler = handler;
    }

    public async ValueTask<int> Run(int input, int constantToAdd, CancellationToken ct)
    {
        var pipeline = _space.CreatePipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear<int, int>("AddConstant", async (val, next, ct2) =>
            {
                var result = val + constantToAdd;
                return await next(result, ct2);
            })
            .HandleWith("TestHandler", _handler)
            .BuildPipeline().Compile();
        return await pipeline(input, ct);
    }
}

public class TestReturningCancellableThreeStepPipeline
{
    private readonly Space _space;
    private readonly Handler<int, int> _handler;

    public TestReturningCancellableThreeStepPipeline(Handler<int, int> handler)
    {
        _space = new Space();
        _handler = handler;
    }

    public async ValueTask<int> Run(int input, int constantToAdd, int multiplier, CancellationToken ct)
    {
        var pipeline = _space.CreatePipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear<int, int>("AddConstant", async (val, next, ct2) =>
            {
                var result = val + constantToAdd;
                return await next(result, ct2);
            })
            .ThenLinear<int, int>("MultiplyByCoefficient", async (val, next, ct2) =>
            {
                var result = val * multiplier;
                return await next(result, ct2);
            })
            .HandleWith("TestHandler", _handler)
            .BuildPipeline().Compile();
        return await pipeline(input, ct);
    }
}

public class TestReturningCancellableIfStepPipeline
{
    private readonly Func<string, int, CancellationToken, ValueTask<int>> _pipeline;

    public TestReturningCancellableIfStepPipeline()
    {
        var space = new Space();
        _pipeline = (input, constantToAdd, ct) =>
        {
            var pipeline = space.CreatePipeline<string, int>("TestIfStepPipeline")
                .StartWithLinear<string, int>("TrimString", async (val, next, ct2) =>
                {
                    var trimmed = val.Trim();
                    return await next(trimmed, ct2);
                })
                .ThenIf<string, int, int, int>("CheckIntOrFloat", async (val, conditionalNext, next, ct2) =>
                {
                    if (int.TryParse(val, out var intValue))
                    {
                        return await next(intValue, ct2);
                    }
                    else
                    {
                        return await conditionalNext(val, ct2);
                    }
                }, s => s.CreatePipeline<string, int>("FloatProcessing")
                    .StartWithLinear<double, int>("ParseFloat", async (val2, next, ct3) =>
                    {
                        if (double.TryParse(val2, out var floatValue))
                        {
                            return await next(floatValue, ct3);
                        }
                        return 0;
                    })
                    .ThenLinear<int, int>("RoundToInt", async (val3, next, ct3) =>
                    {
                        var rounded = (int)Math.Round(val3);
                        return await next(rounded, ct3);
                    })
                    .BuildOpenPipeline())
                .ThenLinear<int, int>("AddConstant", async (val, next, ct2) =>
                {
                    var result = val + constantToAdd;
                    return await next(result, ct2);
                })
                .HandleWith("TestHandler", async (val, ct2) => await Task.FromResult(val))
                .BuildPipeline().Compile();
            return pipeline(input, ct);
        };
    }

    public async ValueTask<int> Run(string input, int constantToAdd, CancellationToken ct)
    {
        return await _pipeline(input, constantToAdd, ct);
    }
}

public class TestReturningCancellableIfElseStepPipeline
{
    private readonly Func<string, int, int, CancellationToken, ValueTask<int>> _pipeline;

    public TestReturningCancellableIfElseStepPipeline()
    {
        var space = new Space();
        _pipeline = (input, constantToAdd, multiplier, ct) =>
        {
            var pipeline = space.CreatePipeline<string, int>("TestIfElseStepPipeline")
                .StartWithLinear<string, int>("TrimString", async (val, next, ct2) =>
                {
                    var trimmed = val.Trim();
                    return await next(trimmed, ct2);
                })
                .ThenIfElse<string, int, int, int, int, int>("CheckIntOrFloat", async (val, trueNext, falseNext, ct2) =>
                {
                    if (int.TryParse(val, out var intValue))
                    {
                        return await falseNext(intValue, ct2);
                    }
                    else
                    {
                        return await trueNext(val, ct2);
                    }
                },
                s => s.CreatePipeline<string, int>("FloatProcessing")
                    .StartWithLinear<double, int>("ParseFloat", async (val2, next, ct3) =>
                    {
                        if (double.TryParse(val2, out var floatValue))
                        {
                            return await next(floatValue, ct3);
                        }
                        return 0;
                    })
                    .ThenLinear<int, int>("RoundToInt", async (val3, next, ct3) =>
                    {
                        var rounded = (int)Math.Round(val3);
                        return await next(rounded, ct3);
                    })
                    .BuildOpenPipeline(),
                s => s.CreatePipeline<int, int>("IntOrDefaultProcessing")
                    .StartWithLinear<int, int>("ParseIntOrDefault", async (val2, next, ct3) =>
                    {
                        return await next(val2 * multiplier, ct3);
                    })
                    .BuildOpenPipeline())
                .ThenLinear<int, int>("AddConstant", async (val, next, ct2) =>
                {
                    var result = val + constantToAdd;
                    return await next(result, ct2);
                })
                .HandleWith("TestHandler", async (val, ct2) => await Task.FromResult(val))
                .BuildPipeline().Compile();
            return pipeline(input, ct);
        };
    }

    public async ValueTask<int> Run(string input, int constantToAdd, int multiplier, CancellationToken ct)
    {
        return await _pipeline(input, constantToAdd, multiplier, ct);
    }
}

public class TestReturningCancellableSwitchStepPipeline
{
    private readonly Func<string, CancellationToken, ValueTask<int>> _pipeline;

    public TestReturningCancellableSwitchStepPipeline()
    {
        var space = new Space();
        var defaultPipeline = space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", async (input, next, ct) =>
            {
                return await next(input, ct);
            })
            .BuildOpenPipeline();

        var compiled = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear<string, int>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed, ct);
            })
            .ThenSwitch<int, int, int, int, int, int>("NumberRangeSwitch", async (input, cases, defaultNext, ct) =>
            {
                if (int.TryParse(input, out var number))
                {
                    if (number > 100) return await cases["GreaterThan100"](number, ct);
                    if (number > 0) return await cases["BetweenZeroAndHundred"](number, ct);
                    if (number < 0) return await cases["LessThanZero"](number, ct);
                    return await cases["EqualToZero"](number, ct);
                }
                return await defaultNext(input.Length, ct);
            },
            s => new Dictionary<string, OpenPipeline<int, int, int, int>>
            {
                ["GreaterThan100"] = s.CreatePipeline<int, int>("MultiplyByThree")
                    .StartWithLinear<int, int>("MultiplyOperation", async (input, next, ct) => await next(input * 3, ct))
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = s.CreatePipeline<int, int>("AddTwo")
                    .StartWithLinear<int, int>("AddOperation", async (input, next, ct) => await next(input + 2, ct))
                    .BuildOpenPipeline(),
                ["LessThanZero"] = s.CreatePipeline<int, int>("MultiplyByTwo")
                    .StartWithLinear<int, int>("MultiplyOperation", async (input, next, ct) => await next(input * 2, ct))
                    .BuildOpenPipeline(),
                ["EqualToZero"] = s.CreatePipeline<int, int>("KeepZero")
                    .StartWithLinear<int, int>("IdentityOperation", async (input, next, ct) => await next(input, ct))
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", async (input, ct) => await Task.FromResult(input))
            .BuildPipeline().Compile();
        
        _pipeline = async (input, ct) => await compiled(input, ct);
    }

    public async ValueTask<int> Run(string input, CancellationToken ct)
    {
        return await _pipeline(input, ct);
    }
}

public class TestReturningCancellableForkStepPipeline
{
    private readonly Func<string, CancellationToken, ValueTask<(int?, string?)>> _pipeline;

    public TestReturningCancellableForkStepPipeline()
    {
        var space = new Space();
        var compiled = space.CreatePipeline<string, (int?, string?)>("TestForkPipeline")
            .StartWithLinear<string, (int?, string?)>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed, ct);
            })
            .ThenFork<string, (int?, string?), string, (int?, string?)>("DigitContentFork", async (input, digitBranch, nonDigitBranch, ct) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                if (containsOnlyDigits)
                {
                    return await digitBranch(input, ct);
                }
                else
                {
                    return await nonDigitBranch(input, ct);
                }
            },
            s => s.CreatePipeline<string, (int?, string?)>("DigitProcessing")
                .StartWithHandler("IntHandler", async (input, ct) => await Task.FromResult<(int?, string?)>((int.Parse(input), null)))
                .BuildPipeline(),
            s => s.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
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
                .BuildPipeline())
            .BuildPipeline().Compile();
        
        _pipeline = async (input, ct) => await compiled(input, ct);
    }

    public async ValueTask<(int?, string?)> Run(string input, CancellationToken ct)
    {
        return await _pipeline(input, ct);
    }
}

public class TestReturningCancellableMultiForkStepPipeline
{
    private readonly Func<string, CancellationToken, ValueTask<(int?, string?, char[]?)>> _pipeline;

    public TestReturningCancellableMultiForkStepPipeline()
    {
        var space = new Space();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", async (input, next, ct) => await next(int.TryParse(input, out var number) ? number : 0, ct))
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", async (input, next, ct) => await next(input + 10, ct))
            .HandleWith("IntHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((input, null, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", async (input, next, ct) => await next($"  {input}  ", ct))
            .HandleWith("StringHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((null, input, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", async (input, next, ct) => await next(input.ToCharArray(), ct))
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", async (input, next, ct) => await next(input.Distinct().ToArray(), ct))
            .HandleWith("CharArrayHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input)))
            .BuildPipeline();
        
        var defaultPipeline = space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithHandler("DefaultHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input.Distinct().ToArray())))
            .BuildPipeline();

        var compiled = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", async (input, next, ct) => await next(input.Trim(), ct))
            .ThenMultiFork<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("ClassifyStringContent", async (input, branches, defaultNext, ct) =>
            {
                if (!string.IsNullOrEmpty(input) && input.All(char.IsDigit)) return await branches["DigitBranch"](input, ct);
                if (!string.IsNullOrEmpty(input) && input.All(char.IsLetter)) return await branches["LetterBranch"](input, ct);
                if (!string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c))) return await branches["SpecialCharBranch"](input, ct);
                return await defaultNext(input.ToCharArray(), ct);
            },
            s => new Dictionary<string, Pipeline<string, (int?, string?, char[]?)>>
            {
                ["DigitBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")!,
                ["LetterBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            s => s.GetPipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")!)
            .BuildPipeline().Compile();
        
        _pipeline = async (input, ct) => await compiled(input, ct);
    }

    public async ValueTask<(int?, string?, char[]?)> Run(string input, CancellationToken ct)
    {
        return await _pipeline(input, ct);
    }
}


