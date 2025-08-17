using K1vs.DotNetPipe.ReturningSync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace K1vs.DotNetPipe.Tests.ReturningSync;

public class TestReturningPipeline
{
    private readonly Pipeline<int, int> _pipeline;

    public TestReturningPipeline(Handler<int, int> handler)
    {
        var space = Pipelines.CreateReturningSyncSpace();
        _pipeline = space.CreatePipeline<int, int>("TestPipeline")
            .StartWithHandler("TestHandler", handler)
            .BuildPipeline();
    }

    public int Run(int input)
    {
        var compiled = _pipeline.Compile();
        return compiled(input);
    }
}

public class TestReturningTwoStepPipeline
{
    private readonly Space _space;
    private readonly Handler<int, int> _handler;

    public TestReturningTwoStepPipeline(Handler<int, int> handler)
    {
        _space = Pipelines.CreateReturningSyncSpace();
        _handler = handler;
    }

    public int Run(int input, int constantToAdd)
    {
        var pipeline = _space.CreatePipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (val, next) =>
            {
                var result = val + constantToAdd;
                return next(result);
            })
            .HandleWith("TestHandler", _handler)
            .BuildPipeline().Compile();
        return pipeline(input);
    }
}

public class TestReturningThreeStepPipeline
{
    private readonly Space _space;
    private readonly Handler<int, int> _handler;

    public TestReturningThreeStepPipeline(Handler<int, int> handler)
    {
        _space = Pipelines.CreateReturningSyncSpace();
        _handler = handler;
    }

    public int Run(int input, int constantToAdd, int multiplier)
    {
        var pipeline = _space.CreatePipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (val, next) =>
            {
                var result = val + constantToAdd;
                return next(result);
            })
            .ThenLinear<int, int>("MultiplyByCoefficient", (val, next) =>
            {
                var result = val * multiplier;
                return next(result);
            })
            .HandleWith("TestHandler", _handler)
            .BuildPipeline().Compile();
        return pipeline(input);
    }
}

public class TestReturningIfStepPipeline
{
    private readonly Func<string, int, int> _pipeline;

    public TestReturningIfStepPipeline()
    {
        var space = Pipelines.CreateReturningSyncSpace();
        _pipeline = (input, constantToAdd) =>
        {
            var pipeline = space.CreatePipeline<string, int>("TestIfStepPipeline")
                .StartWithLinear<string, int>("TrimString", (val, next) =>
                {
                    var trimmed = val.Trim();
                    return next(trimmed);
                })
                .ThenIf<string, int, int, int>("CheckIntOrFloat", (val, conditionalNext, next) =>
                {
                    if (int.TryParse(val, out var intValue))
                    {
                        return next(intValue);
                    }
                    else
                    {
                        return conditionalNext(val);
                    }
                }, s => s.CreatePipeline<string, int>("FloatProcessing")
                    .StartWithLinear<double, int>("ParseFloat", (val, next) =>
                    {
                        if (double.TryParse(val, out var floatValue))
                        {
                            return next(floatValue);
                        }
                        return 0; // Should not happen in this test
                    })
                    .ThenLinear<int, int>("RoundToInt", (val, next) =>
                    {
                        var rounded = (int)Math.Round(val);
                        return next(rounded);
                    })
                    .BuildOpenPipeline())
                .ThenLinear<int, int>("AddConstant", (val, next) =>
                {
                    var result = val + constantToAdd;
                    return next(result);
                })
                .HandleWith("TestHandler", (val) => val)
                .BuildPipeline().Compile();
            return pipeline(input);
        };
    }

    public int Run(string input, int constantToAdd)
    {
        return _pipeline(input, constantToAdd);
    }
}

public class TestReturningIfElseStepPipeline
{
    private readonly Func<string, int, int, int> _pipeline;

    public TestReturningIfElseStepPipeline()
    {
        var space = Pipelines.CreateReturningSyncSpace();
        _pipeline = (input, constantToAdd, multiplier) =>
        {
            var pipeline = space.CreatePipeline<string, int>("TestIfElseStepPipeline")
                .StartWithLinear<string, int>("TrimString", (val, next) =>
                {
                    var trimmed = val.Trim();
                    return next(trimmed);
                })
                .ThenIfElse<string, int, int, int, int, int>("CheckIntOrFloat", (val, trueNext, falseNext) =>
                {
                    if (int.TryParse(val, out var intValue))
                    {
                        return falseNext(intValue);
                    }
                    else
                    {
                        return trueNext(val);
                    }
                },
                s => s.CreatePipeline<string, int>("FloatProcessing")
                    .StartWithLinear<double, int>("ParseFloat", (val, next) =>
                    {
                        if (double.TryParse(val, out var floatValue))
                        {
                            return next(floatValue);
                        }
                        return 0; // Should not happen
                    })
                    .ThenLinear<int, int>("RoundToInt", (val, next) =>
                    {
                        var rounded = (int)Math.Round(val);
                        return next(rounded);
                    })
                    .BuildOpenPipeline(),
                s => s.CreatePipeline<int, int>("IntOrDefaultProcessing")
                    .StartWithLinear<int, int>("ParseIntOrDefault", (val, next) =>
                    {
                        return next(val * multiplier);
                    })
                    .BuildOpenPipeline())
                .ThenLinear<int, int>("AddConstant", (val, next) =>
                {
                    var result = val + constantToAdd;
                    return next(result);
                })
                .HandleWith("TestHandler", (val) => val)
                .BuildPipeline().Compile();
            return pipeline(input);
        };
    }

    public int Run(string input, int constantToAdd, int multiplier)
    {
        return _pipeline(input, constantToAdd, multiplier);
    }
}

public class TestReturningSwitchStepPipeline
{
    private readonly Func<string, int> _pipeline;

    public TestReturningSwitchStepPipeline()
    {
        var space = Pipelines.CreateReturningSyncSpace();
        var defaultPipeline = space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", (input, next) =>
            {
                return next(input);
            })
            .BuildOpenPipeline();

        var compiled = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear<string, int>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                return next(trimmed);
            })
            .ThenSwitch<int, int, int, int, int, int>("NumberRangeSwitch", (input, cases, defaultNext) =>
            {
                if (int.TryParse(input, out var number))
                {
                    if (number > 100) return cases["GreaterThan100"](number);
                    if (number > 0) return cases["BetweenZeroAndHundred"](number);
                    if (number < 0) return cases["LessThanZero"](number);
                    return cases["EqualToZero"](number);
                }
                return defaultNext(input.Length);
            },
            s => new Dictionary<string, OpenPipeline<int, int, int, int>>
            {
                ["GreaterThan100"] = s.CreatePipeline<int, int>("MultiplyByThree")
                    .StartWithLinear<int, int>("MultiplyOperation", (input, next) => next(input * 3))
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = s.CreatePipeline<int, int>("AddTwo")
                    .StartWithLinear<int, int>("AddOperation", (input, next) => next(input + 2))
                    .BuildOpenPipeline(),
                ["LessThanZero"] = s.CreatePipeline<int, int>("MultiplyByTwo")
                    .StartWithLinear<int, int>("MultiplyOperation", (input, next) => next(input * 2))
                    .BuildOpenPipeline(),
                ["EqualToZero"] = s.CreatePipeline<int, int>("KeepZero")
                    .StartWithLinear<int, int>("IdentityOperation", (input, next) => next(input))
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", (input) => input)
            .BuildPipeline().Compile();
        
        _pipeline = (input) => compiled(input);
    }

    public int Run(string input)
    {
        return _pipeline(input);
    }
}

public class TestReturningForkStepPipeline
{
    private readonly Func<string, (int?, string?)> _pipeline;

    public TestReturningForkStepPipeline()
    {
        var space = Pipelines.CreateReturningSyncSpace();
        var compiled = space.CreatePipeline<string, (int?, string?)>("TestForkPipeline")
            .StartWithLinear<string, (int?, string?)>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                return next(trimmed);
            })
            .ThenFork<string, (int?, string?), string, (int?, string?)>("DigitContentFork", (input, digitBranch, nonDigitBranch) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                if (containsOnlyDigits)
                {
                    return digitBranch(input);
                }
                else
                {
                    return nonDigitBranch(input);
                }
            },
            s => s.CreatePipeline<string, (int?, string?)>("DigitProcessing")
                .StartWithHandler("IntHandler", (input) => (int.Parse(input), null))
                .BuildPipeline(),
            s => s.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
                .StartWithLinear<string, (int?, string?)>("RemoveDigits", (input, next) =>
                {
                    var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                    return next(nonDigitsOnly);
                })
                .ThenLinear<string, (int?, string?)>("AddSpaces", (input, next) =>
                {
                    var withSpaces = $"  {input}  ";
                    return next(withSpaces);
                })
                .HandleWith("StringHandler", (input) => (null, input))
                .BuildPipeline())
            .BuildPipeline().Compile();
        
        _pipeline = (input) => compiled(input);
    }

    public (int?, string?) Run(string input)
    {
        return _pipeline(input);
    }
}

public class TestReturningMultiForkStepPipeline
{
    private readonly Func<string, (int?, string?, char[]?)> _pipeline;

    public TestReturningMultiForkStepPipeline()
    {
        var space = Pipelines.CreateReturningSyncSpace();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", (input, next) => next(int.TryParse(input, out var number) ? number : 0))
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", (input, next) => next(input + 10))
            .HandleWith("IntHandler", (input) => (input, null, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", (input, next) => next($"  {input}  "))
            .HandleWith("StringHandler", (input) => (null, input, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", (input, next) => next(input.ToCharArray()))
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", (input, next) => next(input.Distinct().ToArray()))
            .HandleWith("CharArrayHandler", (input) => (null, null, input))
            .BuildPipeline();
        
        var defaultPipeline = space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithHandler("DefaultHandler", (input) => (null, null, input.Distinct().ToArray()))
            .BuildPipeline();

        var compiled = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", (input, next) => next(input.Trim()))
            .ThenMultiFork<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("ClassifyStringContent", (input, branches, defaultNext) =>
            {
                if (!string.IsNullOrEmpty(input) && input.All(char.IsDigit)) return branches["DigitBranch"](input);
                if (!string.IsNullOrEmpty(input) && input.All(char.IsLetter)) return branches["LetterBranch"](input);
                if (!string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c))) return branches["SpecialCharBranch"](input);
                return defaultNext(input.ToCharArray());
            },
            s => new Dictionary<string, Pipeline<string, (int?, string?, char[]?)>>
            {
                ["DigitBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")!,
                ["LetterBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            s => s.GetPipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")!)
            .BuildPipeline().Compile();
        
        _pipeline = (input) => compiled(input);
    }

    public (int?, string?, char[]?) Run(string input)
    {
        return _pipeline(input);
    }
}

