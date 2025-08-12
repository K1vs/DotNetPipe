using K1vs.DotNetPipe.ReturningCancellable;

namespace K1vs.DotNetPipe.Tests.ReturningCancellable;

public class WithoutMutationPipelineTests
{
    [Theory]
    [InlineData(-4, 8)]
    [InlineData(0, 0)]
    [InlineData(2, -4)]
    public async Task BuildAndRunPipeline_WhenOneFuncStep_ShouldReturnResult(int value, int expectedResult)
    {
        var pipeline = new Space()
            .CreatePipeline<int, int>("TestPipeline")
            .StartWithHandler("TestHandler", async (input, ct) =>
            {
                var result = input * -2;
                return await Task.FromResult(result);
            })
            .BuildPipeline().Compile();

        var actualResult = await pipeline(value, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(-4, 5, 2)]
    [InlineData(0, 10, 20)]
    [InlineData(2, 3, 10)]
    public async Task BuildAndRunPipeline_WhenLinearStepThenFuncStep_ShouldReturnResult(int inputValue, int constantToAdd, int expectedResult)
    {
        var pipeline = new Space()
            .CreatePipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear<int, int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                return await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) =>
            {
                var result = input * 2;
                return await Task.FromResult(result);
            })
            .BuildPipeline().Compile();

        var actualResult = await pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(2, 3, 2, 10)]
    [InlineData(0, 5, 3, 15)]
    [InlineData(-1, 4, 2, 6)]
    [InlineData(10, -5, 4, 20)]
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenFuncStep_ShouldReturnResult(int inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        var pipeline = new Space()
            .CreatePipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear<int, int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                return await next(result, ct);
            })
            .ThenLinear<int, int>("MultiplyByCoefficient", async (input, next, ct) =>
            {
                var result = input * multiplier;
                return await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) => await Task.FromResult(input))
            .BuildPipeline().Compile();

        var actualResult = await pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("  5  ", 3, 8)]
    [InlineData(" 10 ", -2, 8)]
    [InlineData("3.7", 5, 9)]
    [InlineData(" 2.3 ", 1, 3)]
    [InlineData("5.5", 2, 8)]
    public async Task BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int constantToAdd, int expectedResult)
    {
        var pipeline = new Space()
            .CreatePipeline<string, int>("TestIfStepPipeline")
            .StartWithLinear<string, int>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed, ct);
            })
            .ThenIf<string, int, int, int>("CheckIntOrFloat", async (input, conditionalNext, next, ct) =>
            {
                if (int.TryParse(input, out var intValue))
                {
                    return await next(intValue, ct);
                }
                else
                {
                    return await conditionalNext(input, ct);
                }
            }, space => space.CreatePipeline<string, int>("FloatProcessing")
                .StartWithLinear<double, int>("ParseFloat", async (input, next, ct) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        return await next(floatValue, ct);
                    }
                    return 0;
                })
                .ThenLinear<int, int>("RoundToInt", async (input, next, ct) =>
                {
                    var rounded = (int)Math.Round(input);
                    return await next(rounded, ct);
                })
                .BuildOpenPipeline())
            .ThenLinear<int, int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                return await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) => await Task.FromResult(input))
            .BuildPipeline().Compile();

        var actualResult = await pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("  5  ", 3, 2, 13)]
    [InlineData(" 10 ", -2, 4, 38)]
    [InlineData("3.7", 5, 3, 9)]
    [InlineData(" 2.3 ", 1, 5, 3)]
    [InlineData("5.5", 2, 7, 8)]
    public async Task BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        var pipeline = new Space()
            .CreatePipeline<string, int>("TestIfElseStepPipeline")
            .StartWithLinear<string, int>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed, ct);
            })
            .ThenIfElse<string, int, int, int, int, int>("CheckIntOrFloat", async (input, trueNext, falseNext, ct) =>
            {
                if (int.TryParse(input, out var intValue))
                {
                    return await falseNext(intValue, ct);
                }
                else
                {
                    return await trueNext(input, ct);
                }
            },
            space => space.CreatePipeline<string, int>("FloatProcessing")
                .StartWithLinear<double, int>("ParseFloat", async (input, next, ct) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        return await next(floatValue, ct);
                    }
                    return 0;
                })
                .ThenLinear<int, int>("RoundToInt", async (input, next, ct) =>
                {
                    var rounded = (int)Math.Round(input);
                    return await next(rounded, ct);
                })
                .BuildOpenPipeline(),
            space => space.CreatePipeline<int, int>("IntOrDefaultProcessing")
                .StartWithLinear<int, int>("ParseIntOrDefault", async (input, next, ct) =>
                {
                    return await next(input * multiplier, ct);
                })
                .BuildOpenPipeline())
            .ThenLinear<int, int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                return await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) => await Task.FromResult(input))
            .BuildPipeline().Compile();

        var actualResult = await pipeline(inputValue, CancellationToken.None);
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
    public async Task BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        var space = new Space();
        var defaultPipeline = space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", async (input, next, ct) =>
            {
                return await next(input, ct);
            })
            .BuildOpenPipeline();

        var pipeline = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear<string, int>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed, ct);
            })
            .ThenSwitch<int, int, int, int, int, int>("NumberRangeSwitch", async (input, cases, defaultNext, ct) =>
            {
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        return await cases["GreaterThan100"](number, ct);
                    }
                    else if (number > 0)
                    {
                        return await cases["BetweenZeroAndHundred"](number, ct);
                    }
                    else if (number < 0)
                    {
                        return await cases["LessThanZero"](number, ct);
                    }
                    else
                    {
                        return await cases["EqualToZero"](number, ct);
                    }
                }
                else
                {
                    var stringLength = input.Length;
                    return await defaultNext(stringLength, ct);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int, int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int, int>("MultiplyByThree")
                    .StartWithLinear<int, int>("MultiplyOperation", async (input, next, ct) =>
                    {
                        var result = input * 3;
                        return await next(result, ct);
                    })
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int, int>("AddTwo")
                    .StartWithLinear<int, int>("AddOperation", async (input, next, ct) =>
                    {
                        var result = input + 2;
                        return await next(result, ct);
                    })
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int, int>("MultiplyByTwo")
                    .StartWithLinear<int, int>("MultiplyOperation", async (input, next, ct) =>
                    {
                        var result = input * 2;
                        return await next(result, ct);
                    })
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int, int>("KeepZero")
                    .StartWithLinear<int, int>("IdentityOperation", async (input, next, ct) =>
                    {
                        return await next(input, ct);
                    })
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", async (input, ct) => await Task.FromResult(input))
            .BuildPipeline().Compile();

        var actualResult = await pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("123", 123, null)]
    [InlineData("  456  ", 456, null)]
    [InlineData("abc123def", null, "  abcdef  ")]
    [InlineData("hello", null, "  hello  ")]
    [InlineData("", null, "    ")]
    [InlineData("!@#", null, "  !@#  ")]
    public async Task BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int? expectedIntResult, string? expectedStringResult)
    {
        var pipeline = new Space()
            .CreatePipeline<string, (int?, string?)>("TestForkPipeline")
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
            space => space.CreatePipeline<string, (int?, string?)>("DigitProcessing")
                .StartWithHandler("IntHandler", async (input, ct) => await Task.FromResult<(int?, string?)>((int.Parse(input), null)))
                .BuildPipeline(),
            space => space.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
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

        var actualResult = await pipeline(inputValue, CancellationToken.None);
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
    public async Task BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int? expectedIntResult,
        string? expectedStringResult,
        char[]? expectedCharArrayResult)
    {
        var space = new Space();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", async (input, next, ct) =>
            {
                if (int.TryParse(input, out var number))
                {
                    return await next(number, ct);
                }
                else
                {
                    return await next(0, ct);
                }
            })
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", async (input, next, ct) =>
            {
                var result = input + 10;
                return await next(result, ct);
            })
            .HandleWith("IntHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((input, null, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", async (input, next, ct) =>
            {
                var withSpaces = $"  {input}  ";
                return await next(withSpaces, ct);
            })
            .HandleWith("StringHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((null, input, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("RemoveWhitespace", async (input, next, ct) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                return await next(noWhitespace, ct);
            })
            .ThenLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", async (input, next, ct) =>
            {
                var charArray = input.ToCharArray();
                return await next(charArray, ct);
            })
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", async (input, next, ct) =>
            {
                var uniqueChars = input.Distinct().ToArray();
                return await next(uniqueChars, ct);
            })
            .HandleWith("CharArrayHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input)))
            .BuildPipeline();
        
        var defaultPipeline = space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithHandler("DefaultHandler", async (input, ct) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input.Distinct().ToArray())))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed, ct);
            })
            .ThenMultiFork<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("ClassifyStringContent", async (input, branches, defaultNext, ct) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (containsOnlyDigits)
                {
                    return await branches["DigitBranch"](input, ct);
                }
                else if (containsOnlyLetters)
                {
                    return await branches["LetterBranch"](input, ct);
                }
                else if (containsOnlySpecialChars)
                {
                    return await branches["SpecialCharBranch"](input, ct);
                }
                else
                {
                    var charArray = input.ToCharArray();
                    return await defaultNext(charArray, ct);
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

        var actualResult = await pipeline(inputValue, CancellationToken.None);
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


