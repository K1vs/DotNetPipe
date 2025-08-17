using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.ReturningSyncCancellable;

namespace K1vs.DotNetPipe.Tests.ReturningSyncCancellable;

public class WithMutationPipelineTests
{
    [Theory]
    [InlineData(-4, -3)]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    public void BuildAndRunPipeline_WhenOneHandlerStep_ShouldReturnMutatedResult(int value, int expectedValue)
    {
        var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<int, int>("TestPipeline")
            .StartWithHandler("TestHandler", (input, ct) => input)
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureTestPipelineMutators); });

        var actualResult = pipeline(value, CancellationToken.None);
        Assert.Equal(expectedValue, actualResult);
    }

    private void ConfigureTestPipelineMutators(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int, int, int>("TestPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, (handler) => (input, ct) => handler(input + 1, ct));
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(-4, 5, -2)]
    [InlineData(0, 10, 11)]
    [InlineData(2, 3, 8)]
    public void BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldReturnMutatedResult(int inputValue, int constantToAdd, int expectedResult)
    {
        var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (input, next, ct) => next(input + constantToAdd, ct))
            .HandleWith("TestHandler", (input, ct) => input)
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureTwoStepPipelineMutators); });

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureTwoStepPipelineMutators(Space space)
    {
        var linearStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestTwoStepPipeline", "AddConstant");
        var linearMutator = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => (input, next, ct) => pipe(input * 2, next, ct));
        linearStep.Mutators.AddMutator(linearMutator, AddingMode.ExactPlace);

        var handlerStep = space.GetRequiredHandlerStep<int, int, int, int>("TestTwoStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, h => (input, ct) => h(input + 1, ct));
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(2, 3, 2, 31)]
    [InlineData(0, 5, 3, 46)]
    [InlineData(-1, 4, 2, 27)]
    [InlineData(10, -5, 4, 61)]
    public void BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldReturnMutatedResult(int inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        var pipeline = Pipelines.CreateReturningSyncCancellablePipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (input, next, ct) => next(input + constantToAdd, ct))
            .ThenLinear<int, int>("MultiplyByCoefficient", (input, next, ct) => next(input * multiplier, ct))
            .HandleWith("TestHandler", (input, ct) => input)
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureThreeStepPipelineMutators); });

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureThreeStepPipelineMutators(Space space)
    {
        var firstLinearStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestThreeStepPipeline", "AddConstant");
        var firstLinearMutator = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, pipe => (input, next, ct) => pipe(input + 5, next, ct));
        firstLinearStep.Mutators.AddMutator(firstLinearMutator, AddingMode.ExactPlace);

        var secondLinearStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestThreeStepPipeline", "MultiplyByCoefficient");
        var secondLinearMutator = new StepMutator<Pipe<int, int, int, int>>("MultiplyByCoefficient", 1, pipe => (input, next, ct) => pipe(input + 5, next, ct));
        secondLinearStep.Mutators.AddMutator(secondLinearMutator, AddingMode.ExactPlace);

        var handlerStep = space.GetRequiredHandlerStep<int, int, int, int>("TestThreeStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, h => (input, ct) => h(input + 1, ct));
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("  5  ", 3, 9)]
    [InlineData(" 10 ", -2, 9)]
    [InlineData("3.7", 5, 10)]
    [InlineData(" 2.3 ", 1, 4)]
    [InlineData("5.5", 2, 8)]
    public void BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldReturnMutatedResult(string inputValue, int constantToAdd, int expectedResult)
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
            }, s => s.CreatePipeline<string, int>("FloatProcessing")
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
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureIfStepPipelineMutators); });

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureIfStepPipelineMutators(Space space)
    {
        var ifStep = space.GetRequiredIfStep<string, int, string, int, string, int, int, int>("TestIfStepPipeline", "CheckIntOrFloat");
        var ifSelectorMutator = new StepMutator<IfSelector<string, int, string, int, int, int>>("CheckIntOrFloatMutator", 1, selector => (input, conditionalNext, next, ct) => conditionalNext(input, ct));
        ifStep.Mutators.AddMutator(ifSelectorMutator, AddingMode.ExactPlace);

        var roundStep = space.GetRequiredLinearStep<string, int, double, int, int, int>("FloatProcessing", "RoundToInt");
        var roundMutator = new StepMutator<Pipe<double, int, int, int>>("RoundToIntMutator", 1, pipe => (input, next, ct) =>
        {
            input += 1;
            var rounded = (int)Math.Round(input);
            return next(rounded, ct);
        });
        roundStep.Mutators.AddMutator(roundMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("  5  ", 3, 2, 10)]
    [InlineData(" 10 ", -2, 4, 10)]
    [InlineData("3.7", 5, 3, 12)]
    [InlineData(" 2.3 ", 1, 5, 12)]
    [InlineData("5.5", 2, 7, 17)]
    public void BuildAndRunPipeline_WhenIfElseStepHandlesIntFloat_ShouldReturnMutatedResult(string inputValue, int constantToAdd, int multiplier, int expectedResult)
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
            s => s.CreatePipeline<string, int>("FloatProcessing")
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
            s => s.CreatePipeline<int, int>("IntProcessing")
                .StartWithLinear<int, int>("Multiply", (input, next, ct) => next(input * multiplier, ct))
                .BuildOpenPipeline())
            .ThenLinear<int, int>("AddConstant", (input, next, ct) => next(input + constantToAdd, ct))
            .HandleWith("TestHandler", (input, ct) => input)
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureIfElseStepPipelineMutators); });

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureIfElseStepPipelineMutators(Space space)
    {
        var ifElseStep = space.GetRequiredIfElseStep<string, int, string, int, string, int, int, int, int, int>("TestIfElseStepPipeline", "CheckIntOrFloat");
        var ifElseSelectorMutator = new StepMutator<IfElseSelector<string, int, string, int, int, int>>("CheckIntOrFloatMutator", 1, selector => (input, trueNext, falseNext, ct) =>
        {
            if (int.TryParse(input, out _))
            {
                return trueNext(input, ct);
            }
            else
            {
                return falseNext(0, ct);
            }
        });
        ifElseStep.Mutators.AddMutator(ifElseSelectorMutator, AddingMode.ExactPlace);

        var roundStep = space.GetRequiredLinearStep<string, int, double, int, int, int>("FloatProcessing", "RoundToInt");
        var roundMutator = new StepMutator<Pipe<double, int, int, int>>("RoundToIntMutator", 1, pipe => (input, next, ct) =>
        {
            input += 1;
            var rounded = (int)Math.Round(input);
            return next(rounded, ct);
        });
        roundStep.Mutators.AddMutator(roundMutator, AddingMode.ExactPlace);

        var intProcessingStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("IntProcessing", "Multiply");
        var intProcessingMutator = new StepMutator<Pipe<int, int, int, int>>("IntProcessingMutator", 1, pipe => (input, next, ct) => pipe(input + 2, next, ct));
        intProcessingStep.Mutators.AddMutator(intProcessingMutator, AddingMode.ExactPlace);

        var handlerStep = space.GetRequiredHandlerStep<string, int, int, int>("TestIfElseStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, h => (input, ct) => h(input + 1, ct));
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(" 105 ", 318)]
    [InlineData(" 50 ", 55)]
    [InlineData(" -5 ", -7)]
    [InlineData(" 0 ", 3)]
    [InlineData("abc", 6)]
    [InlineData("hello", 8)]
    [InlineData("", 3)]
    public void BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldReturnMutatedResult(string inputValue, int expectedResult)
    {
        var space = Pipelines.CreateReturningSyncCancellableSpace();
        var defaultPipeline = space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", (input, next, ct) => next(input, ct))
            .BuildOpenPipeline();

        var pipeline = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear<string, int>("TrimString", (input, next, ct) =>
            {
                var trimmed = input.Trim();
                return next(trimmed, ct);
            })
            .ThenSwitch<int, int, int, int, int, int>("NumberRangeSwitch", (input, cases, defaultNext, ct) =>
            {
                if (int.TryParse(input, out var number))
                {
                    if (number > 100) return cases["GreaterThan100"](number, ct);
                    if (number > 0) return cases["BetweenZeroAndHundred"](number, ct);
                    if (number < 0) return cases["LessThanZero"](number, ct);
                    return cases["EqualToZero"](number, ct);
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
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureSwitchStepPipelineMutators); });

        var actualResult = pipeline(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureSwitchStepPipelineMutators(Space space)
    {
        var trimStep = space.GetRequiredLinearStep<string, int, string, int, string, int>("TestSwitchPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, int, string, int>>("TrimStringMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        var switchStep = space.GetRequiredSwitchStep<string, int, string, int, int, int, int, int, int, int>("TestSwitchPipeline", "NumberRangeSwitch");
        var switchSelectorMutator = new StepMutator<SwitchSelector<string, int, int, int, int, int>>("NumberRangeSwitchMutator", 1, selector => (input, cases, defaultNext, ct) =>
        {
            if (int.TryParse(input, out var number))
            {
                if (number > 50) return cases["GreaterThan100"](number, ct);
                if (number > 0) return cases["BetweenZeroAndHundred"](number, ct);
                return cases["LessThanZero"](number, ct);
            }
            return defaultNext(input.Length, ct);
        });
        switchStep.Mutators.AddMutator(switchSelectorMutator, AddingMode.ExactPlace);

        var multiplyByThreeStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("MultiplyByThree", "MultiplyOperation");
        var multiplyByThreeMutator = new StepMutator<Pipe<int, int, int, int>>("MultiplyByThreeMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        multiplyByThreeStep.Mutators.AddMutator(multiplyByThreeMutator, AddingMode.ExactPlace);

        var addTwoStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("AddTwo", "AddOperation");
        var addTwoMutator = new StepMutator<Pipe<int, int, int, int>>("AddTwoMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        addTwoStep.Mutators.AddMutator(addTwoMutator, AddingMode.ExactPlace);

        var multiplyByTwoStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("MultiplyByTwo", "MultiplyOperation");
        var multiplyByTwoMutator = new StepMutator<Pipe<int, int, int, int>>("MultiplyByTwoMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        multiplyByTwoStep.Mutators.AddMutator(multiplyByTwoMutator, AddingMode.ExactPlace);

        var keepZeroStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("KeepZero", "IdentityOperation");
        var keepZeroMutator = new StepMutator<Pipe<int, int, int, int>>("KeepZeroMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        keepZeroStep.Mutators.AddMutator(keepZeroMutator, AddingMode.ExactPlace);

        var stringLengthStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("StringLengthPipeline", "IdentityOperation");
        var stringLengthMutator = new StepMutator<Pipe<int, int, int, int>>("StringLengthMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        stringLengthStep.Mutators.AddMutator(stringLengthMutator, AddingMode.ExactPlace);

        var handlerStep = space.GetRequiredHandlerStep<string, int, int, int>("TestSwitchPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, handler => (input, ct) => handler(input + 3, ct));
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("123", null, "******")]
    [InlineData("  456  ", null, "******")]
    [InlineData("abc123def", 128, null)]
    [InlineData("hello", 5, null)]
    [InlineData("", null, "******")]
    [InlineData("!@#", null, "***!@#***")]
    public void BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldReturnMutatedResult(string inputValue, int? expectedIntResult, string? expectedStringResult)
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
                .StartWithLinear<string, (int?, string?)>("RemoveNonDigits", (input, next, ct) =>
                {
                    var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                    return next(digitsOnly, ct);
                })
                .ThenLinear<int, (int?, string?)>("ParseToInt", (input, next, ct) =>
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
                .HandleWith("IntHandler", (input, ct) => (input, null))
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
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureForkPipelineMutators); });

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

    private void ConfigureForkPipelineMutators(Space space)
    {
        var trimStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("TestForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("TrimStringMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        var forkStep = space.GetRequiredForkStep<string, (int?, string?), string, (int?, string?), string, (int?, string?), string, (int?, string?)>("TestForkPipeline", "DigitContentFork");
        var forkSelectorMutator = new StepMutator<ForkSelector<string, (int?, string?), string, (int?, string?), string, (int?, string?)>>("DigitContentForkMutator", 1, selector => (input, digitBranch, nonDigitBranch, ct) =>
        {
            if (input.Length > 3)
            {
                return digitBranch(input, ct);
            }
            else
            {
                return nonDigitBranch(input, ct);
            }
        });
        forkStep.Mutators.AddMutator(forkSelectorMutator, AddingMode.ExactPlace);

        var removeNonDigitsStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("DigitProcessing", "RemoveNonDigits");
        var removeNonDigitsMutator = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("RemoveNonDigitsMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        removeNonDigitsStep.Mutators.AddMutator(removeNonDigitsMutator, AddingMode.ExactPlace);

        var parseToIntStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), int, (int?, string?)>("DigitProcessing", "ParseToInt");
        var parseToIntMutator = new StepMutator<Pipe<string, (int?, string?), int, (int?, string?)>>("ParseToIntMutator", 1, pipe => (input, next, ct) =>
        {
            if (int.TryParse(input, out var number))
            {
                return next(number + 5, ct);
            }
            else
            {
                return next(0 + 5, ct);
            }
        });
        parseToIntStep.Mutators.AddMutator(parseToIntMutator, AddingMode.ExactPlace);

        var removeDigitsStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "RemoveDigits");
        var removeDigitsMutator = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("RemoveDigitsMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        removeDigitsStep.Mutators.AddMutator(removeDigitsMutator, AddingMode.ExactPlace);

        var addSpacesStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("AddSpacesMutator", 1, pipe => (input, next, ct) =>
        {
            var withAsterisks = $"***{input}***";
            return next(withAsterisks, ct);
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        var intHandlerStep = space.GetRequiredHandlerStep<string, (int?, string?), int, (int?, string?)>("DigitProcessing", "IntHandler");
        var intHandlerMutator = new StepMutator<Handler<int, (int?, string?)>>("IntHandlerMutator", 1, handler => (input, ct) => handler(input, ct));
        intHandlerStep.Mutators.AddMutator(intHandlerMutator, AddingMode.ExactPlace);

        var stringHandlerStep = space.GetRequiredHandlerStep<string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string, (int?, string?)>>("StringHandlerMutator", 1, handler => (input, ct) => handler(input, ct));
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("123", 138, null, new char[0])]
    [InlineData("  456  ", 471, null, new char[0])]
    [InlineData("abc", null, null, new[] { 'a', 'b', 'c', '_' })]
    [InlineData("xyz", null, null, new[] { 'x', 'y', 'z', '_' })]
    [InlineData("!@#", null, null, new[] { '!', '@', '#', '_' })]
    [InlineData("@@@", null, null, new[] { '@', '_' })]
    [InlineData("a1b2", null, null, new[] { 'a', '1', 'b', '2', '_' })]
    [InlineData("hello123", null, null, new[] { 'h', 'e', 'l', 'o', '1', '2', '3', '_' })]
    [InlineData("12345abc", null, null, new[] { '1', '2', '3', '4', '5', 'a', 'b', 'c', '_' })]
    public void BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldReturnMutatedResult(
        string inputValue,
        int? expectedIntResult,
        string? expectedStringResult,
        char[] expectedCharArrayResult)
    {
        var space = Pipelines.CreateReturningSyncCancellableSpace();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", (input, next, ct) => next(int.TryParse(input, out var n) ? n : 0, ct))
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", (input, next, ct) => next(input + 10, ct))
            .HandleWith("IntHandler", (input, ct) => (input, null, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", (input, next, ct) => next($"  {input}  ", ct))
            .HandleWith("StringHandler", (input, ct) => (null, input, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("RemoveWhitespace", (input, next, ct) => next(new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray()), ct))
            .ThenLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", (input, next, ct) => next(input.ToCharArray(), ct))
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", (input, next, ct) => next(input.Distinct().ToArray(), ct))
            .HandleWith("CharArrayHandler", (input, ct) => (null, null, input))
            .BuildPipeline();

        space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount), (int?, string?, char[]?)>("CountDigitsAndLetters", (input, next, ct) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                return next((digitCount, letterCount), ct);
            })
            .ThenLinear<int, (int?, string?, char[]?)>("CalculateRatio", (input, next, ct) =>
            {
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                return next(ratio, ct);
            })
            .HandleWith("IntHandler", (input, ct) => (input, null, null))
            .BuildPipeline();

        var compiled = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", (input, next, ct) => next(input.Trim(), ct))
            .ThenMultiFork<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("ClassifyStringContent", (input, branches, defaultNext, ct) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
                if (containsOnlyDigits) return branches["DigitBranch"](input, ct);
                if (containsOnlyLetters) return branches["LetterBranch"](input, ct);
                if (containsOnlySpecialChars) return branches["SpecialCharBranch"](input, ct);
                var charArray = input.ToCharArray();
                return defaultNext(charArray, ct);
            },
            s => new Dictionary<string, Pipeline<string, (int?, string?, char[]?)>>
            {
                ["DigitBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")!,
                ["LetterBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = s.GetPipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            s => s.GetPipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")!)
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureMultiForkPipelineMutators); });

        var actualResult = compiled(inputValue, CancellationToken.None);
        if (expectedStringResult != null)
        {
            Assert.Null(actualResult.Item1);
            Assert.Equal(expectedStringResult, actualResult.Item2);
            Assert.Null(actualResult.Item3);
        }
        else if (expectedCharArrayResult.Length > 0)
        {
            Assert.Null(actualResult.Item1);
            Assert.Null(actualResult.Item2);
            Assert.True(actualResult.Item3!.SequenceEqual(expectedCharArrayResult));
        }
        else
        {
            Assert.Equal(expectedIntResult, actualResult.Item1);
            Assert.Null(actualResult.Item2);
            Assert.Null(actualResult.Item3);
        }
    }

    private void ConfigureMultiForkPipelineMutators(Space space)
    {
        var trimStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("TestMultiForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("TrimStringMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        var multiForkStep = space.GetRequiredMultiForkStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("TestMultiForkPipeline", "ClassifyStringContent");
        var selectorMutator = new StepMutator<MultiForkSelector<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("ClassifyStringContentMutator", 1, selector => (input, branches, defaultNext, ct) =>
        {
            var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
            if (containsOnlyDigits) return branches["DigitBranch"](input, ct);
            return branches["SpecialCharBranch"](input, ct);
        });
        multiForkStep.Mutators.AddMutator(selectorMutator, AddingMode.ExactPlace);

        var parseStringToIntStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "ParseStringToInt");
        var parseStringToIntMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>>("ParseStringToIntMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        parseStringToIntStep.Mutators.AddMutator(parseStringToIntMutator, AddingMode.ExactPlace);

        var addConstantStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), int?, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int?, (int?, string?, char[]?), int?, (int?, string?, char[]?)>>("AddConstantMutator", 1, pipe => (input, next, ct) => next((input ?? 0) + 10 + 5, ct));
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        var addSpacesStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("LetterProcessingPipeline", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("AddSpacesMutator", 1, pipe => (input, next, ct) => next($"***{input}***", ct));
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        var removeWhitespaceStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "RemoveWhitespace");
        var removeWhitespaceMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("RemoveWhitespaceMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        removeWhitespaceStep.Mutators.AddMutator(removeWhitespaceMutator, AddingMode.ExactPlace);

        var convertToCharArrayStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "ConvertToCharArray");
        var convertToCharArrayMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("ConvertToCharArrayMutator", 1, pipe => (input, next, ct) => next((input + "_").ToCharArray(), ct));
        convertToCharArrayStep.Mutators.AddMutator(convertToCharArrayMutator, AddingMode.ExactPlace);

        var removeDuplicatesStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), char[], (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "RemoveDuplicates");
        var removeDuplicatesMutator = new StepMutator<Pipe<char[], (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("RemoveDuplicatesMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        removeDuplicatesStep.Mutators.AddMutator(removeDuplicatesMutator, AddingMode.ExactPlace);

        var countStep = space.GetRequiredLinearStep<char[], (int?, string?, char[]?), char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?)>("DefaultProcessingPipeline", "CountDigitsAndLetters");
        var countMutator = new StepMutator<Pipe<char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?)>>("CountDigitsAndLettersMutator", 1, pipe => (input, next, ct) => pipe(input, next, ct));
        countStep.Mutators.AddMutator(countMutator, AddingMode.ExactPlace);

        var calculateRatioStep = space.GetRequiredLinearStep<char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?), int, (int?, string?, char[]?)>("DefaultProcessingPipeline", "CalculateRatio");
        var calculateRatioMutator = new StepMutator<Pipe<(int DigitCount, int LetterCount), (int?, string?, char[]?), int, (int?, string?, char[]?)>>("CalculateRatioMutator", 1, pipe => (input, next, ct) => next((input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount) + 2, ct));
        calculateRatioStep.Mutators.AddMutator(calculateRatioMutator, AddingMode.ExactPlace);

        var intHandlerStepDigit = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "IntHandler");
        var intHandlerMutatorDigit = new StepMutator<Handler<int?, (int?, string?, char[]?)>>("IntHandlerMutatorDigit", 1, handler => (input, ct) => handler(input, ct));
        intHandlerStepDigit.Mutators.AddMutator(intHandlerMutatorDigit, AddingMode.ExactPlace);

        var stringHandlerStep = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("LetterProcessingPipeline", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string, (int?, string?, char[]?)>>("StringHandlerMutator", 1, handler => (input, ct) => handler(input, ct));
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);

        var charArrayHandlerStep = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "CharArrayHandler");
        var charArrayHandlerMutator = new StepMutator<Handler<char[], (int?, string?, char[]?)>>("CharArrayHandlerMutator", 1, handler => (input, ct) => handler(input, ct));
        charArrayHandlerStep.Mutators.AddMutator(charArrayHandlerMutator, AddingMode.ExactPlace);

        var intHandlerStepDefault = space.GetRequiredHandlerStep<char[], (int?, string?, char[]?), int, (int?, string?, char[]?)>("DefaultProcessingPipeline", "IntHandler");
        var intHandlerMutatorDefault = new StepMutator<Handler<int, (int?, string?, char[]?)>>("IntHandlerMutatorDefault", 1, handler => (input, ct) => handler(input, ct));
        intHandlerStepDefault.Mutators.AddMutator(intHandlerMutatorDefault, AddingMode.ExactPlace);
    }
}



