using K1vs.DotNetPipe.ReturningSyncCancellable;

namespace K1vs.DotNetPipe.Tests.ReturningSyncCancellable;

public class WithoutMutationPipelineTests
{
    [Theory]
    [InlineData(-4, 8)]
    [InlineData(0, 0)]
    [InlineData(2, -4)]
    public void BuildAndRunPipeline_WhenOneFuncStep_ShouldReturnResult(int value, int expectedResult)
    {
        var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<int, int>("TestPipeline")
            .StartWithHandler("TestHandler", (input, ct) => input * -2)
            .BuildPipeline().Compile();

        var actualResult = pipeline(value, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(-4, 5, 2)]
    [InlineData(0, 10, 20)]
    [InlineData(2, 3, 10)]
    public void BuildAndRunPipeline_WhenLinearStepThenFuncStep_ShouldReturnResult(int inputValue, int constantToAdd, int expectedResult)
    {
        var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (input, next, ct) => next(input + constantToAdd, ct))
            .HandleWith("TestHandler", (input, ct) => input * 2)
            .BuildPipeline().Compile();

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(2, 3, 2, 10)]
    [InlineData(0, 5, 3, 15)]
    [InlineData(-1, 4, 2, 6)]
    [InlineData(10, -5, 4, 20)]
    public void BuildAndRunPipeline_WhenTwoLinearStepsThenFuncStep_ShouldReturnResult(int inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (input, next, ct) => next(input + constantToAdd, ct))
            .ThenLinear<int, int>("MultiplyByCoefficient", (input, next, ct) => next(input * multiplier, ct))
            .HandleWith("TestHandler", (input, ct) => input)
            .BuildPipeline().Compile();

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("  5  ", 3, 8)]
    [InlineData(" 10 ", -2, 8)]
    [InlineData("3.7", 5, 9)]
    [InlineData(" 2.3 ", 1, 3)]
    [InlineData("5.5", 2, 8)]
    public void BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int constantToAdd, int expectedResult)
    {
        var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<string, int>("TestIfStepPipeline")
            .StartWithLinear<string, int>("TrimString", (input, next, ct) => next(input.Trim(), ct))
            .ThenIf<string, int, int, int>("CheckIntOrFloat", (input, conditionalNext, next, ct) =>
            {
                if (int.TryParse(input, out var intValue))
                {
                    return next(intValue, ct);
                }
                else
                {
                    return conditionalNext(input, ct);
                }
            }, space => space.CreatePipeline<string, int>("FloatProcessing")
                .StartWithLinear<double, int>("ParseFloat", (input, next, ct) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        return next(floatValue, ct);
                    }
                    return 0;
                })
                .ThenLinear<int, int>("RoundToInt", (input, next, ct) =>
                {
                    var rounded = (int)Math.Round(input);
                    return next(rounded, ct);
                })
                .BuildOpenPipeline())
            .ThenLinear<int, int>("AddConstant", (input, next, ct) => next(input + constantToAdd, ct))
            .HandleWith("TestHandler", (input, ct) => input)
            .BuildPipeline().Compile();

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("  5  ", 3, 2, 13)]
    [InlineData(" 10 ", -2, 4, 38)]
    [InlineData("3.7", 5, 3, 9)]
    [InlineData(" 2.3 ", 1, 5, 3)]
    [InlineData("5.5", 2, 7, 8)]
    public void BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<string, int>("TestIfElseStepPipeline")
            .StartWithLinear<string, int>("TrimString", (input, next, ct) => next(input.Trim(), ct))
            .ThenIfElse<string, int, int, int, int, int>("CheckIntOrFloat", (input, trueNext, falseNext, ct) =>
            {
                if (int.TryParse(input, out var intValue))
                {
                    return falseNext(intValue, ct);
                }
                else
                {
                    return trueNext(input, ct);
                }
            },
            space => space.CreatePipeline<string, int>("FloatProcessing")
                .StartWithLinear<double, int>("ParseFloat", (input, next, ct) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        return next(floatValue, ct);
                    }
                    return 0;
                })
                .ThenLinear<int, int>("RoundToInt", (input, next, ct) =>
                {
                    var rounded = (int)Math.Round(input);
                    return next(rounded, ct);
                })
                .BuildOpenPipeline(),
            space => space.CreatePipeline<int, int>("IntOrDefaultProcessing")
                .StartWithLinear<int, int>("ParseIntOrDefault", (input, next, ct) => next(input * multiplier, ct))
                .BuildOpenPipeline())
            .ThenLinear<int, int>("AddConstant", (input, next, ct) => next(input + constantToAdd, ct))
            .HandleWith("TestHandler", (input, ct) => input)
            .BuildPipeline().Compile();

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("105", 315)]
    [InlineData("50", 52)]
    [InlineData("-5", -10)]
    [InlineData("0", 0)]
    [InlineData("abc", 3)]
    [InlineData("hello", 5)]
    [InlineData("", 0)]
    public void BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        var space = Pipelines.CreateReturningSyncCancellableSpace();
        var defaultPipeline = space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", (input, next, ct) => next(input, ct))
            .BuildOpenPipeline();

        var pipeline = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear<string, int>("TrimString", (input, next, ct) => next(input.Trim(), ct))
            .ThenSwitch<int, int, int, int, int, int>("NumberRangeSwitch", (input, cases, defaultNext, ct) =>
            {
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        return cases["GreaterThan100"](number, ct);
                    }
                    else if (number > 0)
                    {
                        return cases["BetweenZeroAndHundred"](number, ct);
                    }
                    else if (number < 0)
                    {
                        return cases["LessThanZero"](number, ct);
                    }
                    else
                    {
                        return cases["EqualToZero"](number, ct);
                    }
                }
                else
                {
                    var stringLength = input.Length;
                    return defaultNext(stringLength, ct);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int, int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int, int>("MultiplyByThree")
                    .StartWithLinear<int, int>("MultiplyOperation", (input, next, ct) => next(input * 3, ct))
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int, int>("AddTwo")
                    .StartWithLinear<int, int>("AddOperation", (input, next, ct) => next(input + 2, ct))
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int, int>("MultiplyByTwo")
                    .StartWithLinear<int, int>("MultiplyOperation", (input, next, ct) => next(input * 2, ct))
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int, int>("KeepZero")
                    .StartWithLinear<int, int>("IdentityOperation", (input, next, ct) => next(input, ct))
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", (input, ct) => input)
            .BuildPipeline().Compile();

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("123", 123, null)]
    [InlineData("  456  ", 456, null)]
    [InlineData("abc123def", null, "  abcdef  ")]
    [InlineData("hello", null, "  hello  ")]
    [InlineData("", null, "    ")]
    [InlineData("!@#", null, "  !@#  ")]
    public void BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int? expectedIntResult, string? expectedStringResult)
    {
        var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<string, (int?, string?)>("TestForkPipeline")
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
            space => space.CreatePipeline<string, (int?, string?)>("DigitProcessing")
                .StartWithHandler("IntHandler", (input, ct) => (int.Parse(input), null))
                .BuildPipeline(),
            space => space.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
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

        var actualResult = pipeline(inputValue, CancellationToken.None);
        if (expectedIntResult.HasValue)
        {
            Assert.Equal(expectedIntResult, actualResult.Item1);
            Assert.Null(actualResult.Item2);
        }
        else
        {
            Assert.Null(actualResult.Item1);
            Assert.Equal(expectedStringResult, actualResult.Item2);
        }
    }

    [Theory]
    [InlineData("123", 133, null, null)]
    [InlineData("  456  ", 466, null, null)]
    [InlineData("abc", null, "  abc  ", null)]
    [InlineData("xyz", null, "  xyz  ", null)]
    [InlineData("!@#", null, null, new char[] { '!', '@', '#' })]
    [InlineData("@@@", null, null, new char[] { '@' })]
    [InlineData("hello123", null, null, new char[] { 'h', 'e', 'l', 'o', '1', '2', '3' })]
    public void BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int? expectedIntResult,
        string? expectedStringResult,
        char[]? expectedCharArrayResult)
    {
        var space = Pipelines.CreateReturningSyncCancellableSpace();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", (input, next, ct) =>
            {
                if (int.TryParse(input, out var number))
                {
                    return next(number, ct);
                }
                else
                {
                    return next(0, ct);
                }
            })
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", (input, next, ct) => next(input + 10, ct))
            .HandleWith("IntHandler", (input, ct) => (input, null, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", (input, next, ct) => next($"  {input}  ", ct))
            .HandleWith("StringHandler", (input, ct) => (null, input, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("RemoveWhitespace", (input, next, ct) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                return next(noWhitespace, ct);
            })
            .ThenLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", (input, next, ct) => next(input.ToCharArray(), ct))
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", (input, next, ct) => next(input.Distinct().ToArray(), ct))
            .HandleWith("CharArrayHandler", (input, ct) => (null, null, input))
            .BuildPipeline();
        
        var defaultPipeline = space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithHandler("DefaultHandler", (input, ct) => (null, null, input.Distinct().ToArray()))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", (input, next, ct) => next(input.Trim(), ct))
            .ThenMultiFork<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("ClassifyStringContent", (input, branches, defaultNext, ct) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
                
                if (containsOnlyDigits)
                {
                    return branches["DigitBranch"](input, ct);
                }
                else if (containsOnlyLetters)
                {
                    return branches["LetterBranch"](input, ct);
                }
                else if (containsOnlySpecialChars)
                {
                    return branches["SpecialCharBranch"](input, ct);
                }
                else
                {
                    var charArray = input.ToCharArray();
                    return defaultNext(charArray, ct);
                }
            },
            space => new Dictionary<string, Pipeline<string, (int?, string?, char[]?)>>
            {
                ["DigitBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")!,
                ["LetterBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            space => space.GetPipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")!)
            .BuildPipeline().Compile();

        var actualResult = pipeline(inputValue, CancellationToken.None);
        if (expectedIntResult.HasValue)
        {
            Assert.Equal(expectedIntResult, actualResult.Item1);
            Assert.Null(actualResult.Item2);
            Assert.Null(actualResult.Item3);
        }
        else if (expectedStringResult != null)
        {
            Assert.Null(actualResult.Item1);
            Assert.Equal(expectedStringResult, actualResult.Item2);
            Assert.Null(actualResult.Item3);
        }
        else
        {
            Assert.Null(actualResult.Item1);
            Assert.Null(actualResult.Item2);
            Assert.Equal(expectedCharArrayResult, actualResult.Item3);
        }
    }
}



