using K1vs.DotNetPipe.Returning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K1vs.DotNetPipe.Tests.Returning;

public class TestReturningPipeline
{
    private readonly Pipeline<int, int> _pipeline;

    public TestReturningPipeline(Handler<int, int> handler)
    {
        var space = Pipelines.CreateReturningSpace();
        _pipeline = space.CreatePipeline<int, int>("TestPipeline")
            .StartWithHandler("TestHandler", handler)
            .BuildPipeline();
    }

    public async ValueTask<int> Run(int input)
    {
        var compiled = _pipeline.Compile();
        return await compiled(input);
    }
}

public class TestReturningTwoStepPipeline
{
    private readonly Space _space;
    private readonly Handler<int, int> _handler;

    public TestReturningTwoStepPipeline(Handler<int, int> handler)
    {
        _space = Pipelines.CreateReturningSpace();
        _handler = handler;
    }

    public async ValueTask<int> Run(int input, int constantToAdd)
    {
        var pipeline = _space.CreatePipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear<int, int>("AddConstant", async (val, next) =>
            {
                var result = val + constantToAdd;
                return await next(result);
            })
            .HandleWith("TestHandler", _handler)
            .BuildPipeline().Compile();
        return await pipeline(input);
    }
}

public class TestReturningThreeStepPipeline
{
    private readonly Space _space;
    private readonly Handler<int, int> _handler;

    public TestReturningThreeStepPipeline(Handler<int, int> handler)
    {
        _space = Pipelines.CreateReturningSpace();
        _handler = handler;
    }

    public async ValueTask<int> Run(int input, int constantToAdd, int multiplier)
    {
        var pipeline = _space.CreatePipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear<int, int>("AddConstant", async (val, next) =>
            {
                var result = val + constantToAdd;
                return await next(result);
            })
            .ThenLinear<int, int>("MultiplyByCoefficient", async (val, next) =>
            {
                var result = val * multiplier;
                return await next(result);
            })
            .HandleWith("TestHandler", _handler)
            .BuildPipeline().Compile();
        return await pipeline(input);
    }
}

public class TestReturningIfStepPipeline
{
    private readonly Func<string, int, ValueTask<int>> _pipeline;

    public TestReturningIfStepPipeline()
    {
        var space = Pipelines.CreateReturningSpace();
        _pipeline = (input, constantToAdd) =>
        {
            var pipeline = space.CreatePipeline<string, int>("TestIfStepPipeline")
                .StartWithLinear<string, int>("TrimString", async (val, next) =>
                {
                    var trimmed = val.Trim();
                    return await next(trimmed);
                })
                .ThenIf<string, int, int, int>("CheckIntOrFloat", async (val, conditionalNext, next) =>
                {
                    if (int.TryParse(val, out var intValue))
                    {
                        return await next(intValue);
                    }
                    else
                    {
                        return await conditionalNext(val);
                    }
                }, s => s.CreatePipeline<string, int>("FloatProcessing")
                    .StartWithLinear<double, int>("ParseFloat", async (val, next) =>
                    {
                        if (double.TryParse(val, out var floatValue))
                        {
                            return await next(floatValue);
                        }
                        return 0; // Should not happen in this test
                    })
                    .ThenLinear<int, int>("RoundToInt", async (val, next) =>
                    {
                        var rounded = (int)Math.Round(val);
                        return await next(rounded);
                    })
                    .BuildOpenPipeline())
                .ThenLinear<int, int>("AddConstant", async (val, next) =>
                {
                    var result = val + constantToAdd;
                    return await next(result);
                })
                .HandleWith("TestHandler", async (val) => await Task.FromResult(val))
                .BuildPipeline().Compile();
            return pipeline(input);
        };
    }

    public async ValueTask<int> Run(string input, int constantToAdd)
    {
        return await _pipeline(input, constantToAdd);
    }
}

public class TestReturningIfElseStepPipeline
{
    private readonly Func<string, int, int, ValueTask<int>> _pipeline;

    public TestReturningIfElseStepPipeline()
    {
        var space = Pipelines.CreateReturningSpace();
        _pipeline = (input, constantToAdd, multiplier) =>
        {
            var pipeline = space.CreatePipeline<string, int>("TestIfElseStepPipeline")
                .StartWithLinear<string, int>("TrimString", async (val, next) =>
                {
                    var trimmed = val.Trim();
                    return await next(trimmed);
                })
                .ThenIfElse<string, int, int, int, int, int>("CheckIntOrFloat", async (val, trueNext, falseNext) =>
                {
                    if (int.TryParse(val, out var intValue))
                    {
                        return await falseNext(intValue);
                    }
                    else
                    {
                        return await trueNext(val);
                    }
                },
                s => s.CreatePipeline<string, int>("FloatProcessing")
                    .StartWithLinear<double, int>("ParseFloat", async (val, next) =>
                    {
                        if (double.TryParse(val, out var floatValue))
                        {
                            return await next(floatValue);
                        }
                        return 0; // Should not happen
                    })
                    .ThenLinear<int, int>("RoundToInt", async (val, next) =>
                    {
                        var rounded = (int)Math.Round(val);
                        return await next(rounded);
                    })
                    .BuildOpenPipeline(),
                s => s.CreatePipeline<int, int>("IntOrDefaultProcessing")
                    .StartWithLinear<int, int>("ParseIntOrDefault", async (val, next) =>
                    {
                        return await next(val * multiplier);
                    })
                    .BuildOpenPipeline())
                .ThenLinear<int, int>("AddConstant", async (val, next) =>
                {
                    var result = val + constantToAdd;
                    return await next(result);
                })
                .HandleWith("TestHandler", async (val) => await Task.FromResult(val))
                .BuildPipeline().Compile();
            return pipeline(input);
        };
    }

    public async ValueTask<int> Run(string input, int constantToAdd, int multiplier)
    {
        return await _pipeline(input, constantToAdd, multiplier);
    }
}

public class TestReturningSwitchStepPipeline
{
    private readonly Func<string, ValueTask<int>> _pipeline;

    public TestReturningSwitchStepPipeline()
    {
        var space = Pipelines.CreateReturningSpace();
        var defaultPipeline = space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", async (input, next) =>
            {
                return await next(input);
            })
            .BuildOpenPipeline();

        var compiled = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear<string, int>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed);
            })
            .ThenSwitch<int, int, int, int, int, int>("NumberRangeSwitch", async (input, cases, defaultNext) =>
            {
                if (int.TryParse(input, out var number))
                {
                    if (number > 100) return await cases["GreaterThan100"](number);
                    if (number > 0) return await cases["BetweenZeroAndHundred"](number);
                    if (number < 0) return await cases["LessThanZero"](number);
                    return await cases["EqualToZero"](number);
                }
                return await defaultNext(input.Length);
            },
            s => new Dictionary<string, OpenPipeline<int, int, int, int>>
            {
                ["GreaterThan100"] = s.CreatePipeline<int, int>("MultiplyByThree")
                    .StartWithLinear<int, int>("MultiplyOperation", async (input, next) => await next(input * 3))
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = s.CreatePipeline<int, int>("AddTwo")
                    .StartWithLinear<int, int>("AddOperation", async (input, next) => await next(input + 2))
                    .BuildOpenPipeline(),
                ["LessThanZero"] = s.CreatePipeline<int, int>("MultiplyByTwo")
                    .StartWithLinear<int, int>("MultiplyOperation", async (input, next) => await next(input * 2))
                    .BuildOpenPipeline(),
                ["EqualToZero"] = s.CreatePipeline<int, int>("KeepZero")
                    .StartWithLinear<int, int>("IdentityOperation", async (input, next) => await next(input))
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", async (input) => await Task.FromResult(input))
            .BuildPipeline().Compile();
        
        _pipeline = async (input) => await compiled(input);
    }

    public async ValueTask<int> Run(string input)
    {
        return await _pipeline(input);
    }
}

public class TestReturningForkStepPipeline
{
    private readonly Func<string, ValueTask<(int?, string?)>> _pipeline;

    public TestReturningForkStepPipeline()
    {
        var space = Pipelines.CreateReturningSpace();
        var compiled = space.CreatePipeline<string, (int?, string?)>("TestForkPipeline")
            .StartWithLinear<string, (int?, string?)>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed);
            })
            .ThenFork<string, (int?, string?), string, (int?, string?)>("DigitContentFork", async (input, digitBranch, nonDigitBranch) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                if (containsOnlyDigits)
                {
                    return await digitBranch(input);
                }
                else
                {
                    return await nonDigitBranch(input);
                }
            },
            s => s.CreatePipeline<string, (int?, string?)>("DigitProcessing")
                .StartWithHandler("IntHandler", async (input) => await Task.FromResult<(int?, string?)>((int.Parse(input), null)))
                .BuildPipeline(),
            s => s.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
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
                .BuildPipeline())
            .BuildPipeline().Compile();
        
        _pipeline = async (input) => await compiled(input);
    }

    public async ValueTask<(int?, string?)> Run(string input)
    {
        return await _pipeline(input);
    }
}

public class TestReturningMultiForkStepPipeline
{
    private readonly Func<string, ValueTask<(int?, string?, char[]?)>> _pipeline;

    public TestReturningMultiForkStepPipeline()
    {
        var space = Pipelines.CreateReturningSpace();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", async (input, next) => await next(int.TryParse(input, out var number) ? number : 0))
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", async (input, next) => await next(input + 10))
            .HandleWith("IntHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((input, null, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", async (input, next) => await next($"  {input}  "))
            .HandleWith("StringHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((null, input, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", async (input, next) => await next(input.ToCharArray()))
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", async (input, next) => await next(input.Distinct().ToArray()))
            .HandleWith("CharArrayHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input)))
            .BuildPipeline();
        
        var defaultPipeline = space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithHandler("DefaultHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input.Distinct().ToArray())))
            .BuildPipeline();

        var compiled = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", async (input, next) => await next(input.Trim()))
            .ThenMultiFork<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("ClassifyStringContent", async (input, branches, defaultNext) =>
            {
                if (!string.IsNullOrEmpty(input) && input.All(char.IsDigit)) return await branches["DigitBranch"](input);
                if (!string.IsNullOrEmpty(input) && input.All(char.IsLetter)) return await branches["LetterBranch"](input);
                if (!string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c))) return await branches["SpecialCharBranch"](input);
                return await defaultNext(input.ToCharArray());
            },
            s => new Dictionary<string, Pipeline<string, (int?, string?, char[]?)>>
            {
                ["DigitBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")!,
                ["LetterBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            s => s.GetPipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")!)
            .BuildPipeline().Compile();
        
        _pipeline = async (input) => await compiled(input);
    }

    public async ValueTask<(int?, string?, char[]?)> Run(string input)
    {
        return await _pipeline(input);
    }
}
