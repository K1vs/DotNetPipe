using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.ReturningSync;
using Xunit;

namespace K1vs.DotNetPipe.Tests.ReturningSync;

public class WithMutationPipelineTests
{
    [Theory]
    [InlineData(-4, -3)]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    public void BuildAndRunPipeline_WhenOneHandlerStep_ShouldReturnMutatedResult(int value, int expectedValue)
    {
        // Arrange
        var pipeline = Pipelines.CreateReturningSyncPipeline<int, int>("TestPipeline")
            .StartWithHandler("TestHandler", (input) => input)
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureTestPipelineMutators);
            });

        // Act
        var actualResult = pipeline(value);

        // Assert
        Assert.Equal(expectedValue, actualResult);
    }

    private void ConfigureTestPipelineMutators(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int, int, int>("TestPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                input += 1;
                return handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(-4, 5, -2)]   // ((-4 * 2) + 5) + 1 = -2
    [InlineData(0, 10, 11)]  // ((0 * 2) + 10) + 1 = 11
    [InlineData(2, 3, 8)]    // ((2 * 2) + 3) + 1 = 8
    public void BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldReturnMutatedResult(int inputValue, int constantToAdd, int expectedResult)
    {
        // Arrange
        var pipeline = Pipelines.CreateReturningSyncPipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (input, next) =>
            {
                var result = input + constantToAdd;
                return next(result);
            })
            .HandleWith("TestHandler", (input) => input)
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureTwoStepPipelineMutators);
            });

        // Act
        var actualResult = pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureTwoStepPipelineMutators(Space space)
    {
        // Mutator for linear step - multiply input by 2 before processing
        var linearStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestTwoStepPipeline", "AddConstant");
        var linearMutator = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                // Apply mutation to input first - multiply by 2
                input *= 2;
                return pipe(input, next);
            };
        });
        linearStep.Mutators.AddMutator(linearMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1
        var handlerStep = space.GetRequiredHandlerStep<int, int, int, int>("TestTwoStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                input += 1;
                return handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(2, 3, 2, 31)]   // Step1 mut: 2+5=7; Step1: 7+3=10; Step2 mut: 10+5=15; Step2: 15*2=30; Handler mut: 30+1=31
    [InlineData(0, 5, 3, 46)]   // Step1 mut: 0+5=5; Step1: 5+5=10; Step2 mut: 10+5=15; Step2: 15*3=45; Handler mut: 45+1=46
    [InlineData(-1, 4, 2, 27)]  // Step1 mut: -1+5=4; Step1: 4+4=8; Step2 mut: 8+5=13; Step2: 13*2=26; Handler mut: 26+1=27
    [InlineData(10, -5, 4, 61)] // Step1 mut: 10+5=15; Step1: 15-5=10; Step2 mut: 10+5=15; Step2: 15*4=60; Handler mut: 60+1=61
    public void BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldReturnMutatedResult(int inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        // Arrange
        var pipeline = Pipelines.CreateReturningSyncPipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear<int, int>("AddConstant", (input, next) =>
            {
                var result = input + constantToAdd;
                return next(result);
            })
            .ThenLinear<int, int>("MultiplyByCoefficient", (input, next) =>
            {
                var result = input * multiplier;
                return next(result);
            })
            .HandleWith("TestHandler", (input) => input)
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureThreeStepPipelineMutators);
            });

        // Act
        var actualResult = pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureThreeStepPipelineMutators(Space space)
    {
        // Mutator for first linear step (AddConstant) - add 5 to input
        var firstLinearStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestThreeStepPipeline", "AddConstant");
        var firstLinearMutator = new StepMutator<Pipe<int, int, int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                input += 5;
                return pipe(input, next);
            };
        });
        firstLinearStep.Mutators.AddMutator(firstLinearMutator, AddingMode.ExactPlace);

        // Mutator for second linear step (MultiplyByCoefficient) - add 5 to input before multiplying
        var secondLinearStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("TestThreeStepPipeline", "MultiplyByCoefficient");
        var secondLinearMutator = new StepMutator<Pipe<int, int, int, int>>("MultiplyByCoefficient", 1, (pipe) =>
        {
            return (input, next) =>
            {
                input += 5;
                return pipe(input, next);
            };
        });
        secondLinearStep.Mutators.AddMutator(secondLinearMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1
        var handlerStep = space.GetRequiredHandlerStep<int, int, int, int>("TestThreeStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                input += 1;
                return handler(input);
            };
        });
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
        // Arrange
        var pipeline = Pipelines.CreateReturningSyncPipeline<string, int>("TestIfStepPipeline")
            .StartWithLinear<string, int>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                return next(trimmed);
            })
            .ThenIf("CheckIntOrFloat", (input, conditionalNext, next) =>
            {
                if (int.TryParse(input, out var intValue))
                {
                    return next(intValue);
                }
                else
                {
                    return conditionalNext(input);
                }
            }, s => s.CreatePipeline<string, int>("FloatProcessing")
                .StartWithLinear<double, int>("ParseFloat", (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        return next(floatValue);
                    }
                    return 0; // Should not happen
                })
                .ThenLinear<int, int>("RoundToInt", (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    return next(rounded);
                })
                .BuildOpenPipeline())
            .ThenLinear<int, int>("AddConstant", (input, next) =>
            {
                var result = input + constantToAdd;
                return next(result);
            })
            .HandleWith("TestHandler", (input) => input)
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureIfStepPipelineMutators);
            });

        // Act
        var actualResult = pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureIfStepPipelineMutators(Space space)
    {
        // Mutator for the If step selector - force all inputs into the float processing pipeline
        var ifStep = space.GetRequiredIfStep<string, int, string, int, string, int, int, int>("TestIfStepPipeline", "CheckIntOrFloat");
        var ifSelectorMutator = new StepMutator<IfSelector<string, int, string, int, int, int>>("CheckIntOrFloatMutator", 1, (selector) =>
        {
            return (input, conditionalNext, next) =>
            {
                // Modified logic: always go to conditional pipeline (for float parsing)
                return conditionalNext(input);
            };
        });
        ifStep.Mutators.AddMutator(ifSelectorMutator, AddingMode.ExactPlace);

        // Mutator for the RoundToInt step in the conditional pipeline - add 1 to input before rounding
        var roundStep = space.GetRequiredLinearStep<string, int, double, int, int, int>("FloatProcessing", "RoundToInt");
        var roundMutator = new StepMutator<Pipe<double, int, int, int>>("RoundToIntMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                // Add 1 to the input before rounding
                input += 1;
                var rounded = (int)Math.Round(input);
                return next(rounded);
            };
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
        // Arrange
        var pipeline = Pipelines.CreateReturningSyncPipeline<string, int>("TestIfElseStepPipeline")
            .StartWithLinear<string, int>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                return next(trimmed);
            })
            .ThenIfElse("CheckIntOrFloat", (input, trueNext, falseNext) =>
                {
                    if (int.TryParse(input, out var intValue))
                    {
                        return falseNext(intValue); // int goes to false branch
                    }
                    else
                    {
                        return trueNext(input); // float string goes to true branch
                    }
                },
                s => s.CreatePipeline<string, int>("FloatProcessing")
                    .StartWithLinear<double, int>("ParseFloat", (input, next) =>
                    {
                        if (double.TryParse(input, out var floatValue))
                        {
                            return next(floatValue);
                        }
                        return 0;
                    })
                    .ThenLinear<int, int>("RoundToInt", (input, next) =>
                    {
                        var rounded = (int)Math.Round(input);
                        return next(rounded);
                    })
                    .BuildOpenPipeline(),
                s => s.CreatePipeline<int, int>("IntProcessing")
                    .StartWithLinear<int, int>("Multiply", (input, next) =>
                    {
                        return next(input * multiplier);
                    })
                    .BuildOpenPipeline())
            .ThenLinear<int, int>("AddConstant", (input, next) =>
            {
                var result = input + constantToAdd;
                return next(result);
            })
            .HandleWith("TestHandler", (input) => input)
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureIfElseStepPipelineMutators); });

        // Act
        var actualResult = pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureIfElseStepPipelineMutators(Space space)
    {
        var ifElseStep = space.GetRequiredIfElseStep<string, int, string, int, string, int, int, int, int, int>("TestIfElseStepPipeline", "CheckIntOrFloat");
        var ifElseSelectorMutator = new StepMutator<IfElseSelector<string, int, string, int, int, int>>("CheckIntOrFloatMutator", 1, (selector) =>
        {
            return (input, trueNext, falseNext) =>
            {
                // Swap the branches
                if (int.TryParse(input, out _))
                {
                    return trueNext(input); // int now goes to float pipeline
                }
                else
                {
                    return falseNext(0); // float now goes to int pipeline with default 0
                }
            };
        });
        ifElseStep.Mutators.AddMutator(ifElseSelectorMutator, AddingMode.ExactPlace);

        var roundStep = space.GetRequiredLinearStep<string, int, double, int, int, int>("FloatProcessing", "RoundToInt");
        var roundMutator = new StepMutator<Pipe<double, int, int, int>>("RoundToIntMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                input += 1;
                var rounded = (int)Math.Round(input);
                return next(rounded);
            };
        });
        roundStep.Mutators.AddMutator(roundMutator, AddingMode.ExactPlace);

        var intProcessingStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("IntProcessing", "Multiply");
        var intProcessingMutator = new StepMutator<Pipe<int, int, int, int>>("IntProcessingMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                input += 2;
                return pipe(input, next);
            };
        });
        intProcessingStep.Mutators.AddMutator(intProcessingMutator, AddingMode.ExactPlace);

        var handlerStep = space.GetRequiredHandlerStep<string, int, int, int>("TestIfElseStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                input += 1;
                return handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(" 105 ", 318)]    // >50 -> GreaterThan100 -> *3 = 315 -> +3 = 318
    [InlineData(" 50 ", 55)]      // 0<x<=50 -> BetweenZeroAndHundred -> +2 = 52 -> +3 = 55
    [InlineData(" -5 ", -7)]      // <=0 -> LessThanZero -> *2 = -10 -> +3 = -7
    [InlineData(" 0 ", 3)]        // <=0 -> LessThanZero -> *2 = 0 -> +3 = 3
    [InlineData("abc", 6)]        // not a number -> string length = 3 -> +3 = 6
    [InlineData("hello", 8)]      // not a number -> string length = 5 -> +3 = 8
    [InlineData("", 3)]           // empty string -> length = 0 -> +3 = 3
    public void BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldReturnMutatedResult(string inputValue, int expectedResult)
    {
        // Arrange
        var space = Pipelines.CreateReturningSyncSpace();
        var defaultPipeline = space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", (input, next) =>
            {
                return next(input);
            })
            .BuildOpenPipeline();

        var pipeline = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear<string, int>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                return next(trimmed);
            })
            .ThenSwitch<int, int, int, int, int, int>("NumberRangeSwitch", (input, cases, defaultNext) =>
            {
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        return cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        return cases["BetweenZeroAndHundred"](number);
                    }
                    else if (number < 0)
                    {
                        return cases["LessThanZero"](number);
                    }
                    else
                    {
                        return cases["EqualToZero"](number);
                    }
                }
                else
                {
                    var stringLength = input.Length;
                    return defaultNext(stringLength);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int, int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int, int>("MultiplyByThree")
                    .StartWithLinear<int, int>("MultiplyOperation", (input, next) =>
                    {
                        var result = input * 3;
                        return next(result);
                    })
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int, int>("AddTwo")
                    .StartWithLinear<int, int>("AddOperation", (input, next) =>
                    {
                        var result = input + 2;
                        return next(result);
                    })
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int, int>("MultiplyByTwo")
                    .StartWithLinear<int, int>("MultiplyOperation", (input, next) =>
                    {
                        var result = input * 2;
                        return next(result);
                    })
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int, int>("KeepZero")
                    .StartWithLinear<int, int>("IdentityOperation", (input, next) =>
                    {
                        return next(input);
                    })
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", (input) => input)
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureSwitchStepPipelineMutators);
            });

        // Act
        var actualResult = pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    private void ConfigureSwitchStepPipelineMutators(Space space)
    {
        // TrimString step - pass through
        var trimStep = space.GetRequiredLinearStep<string, int, string, int, string, int>("TestSwitchPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, int, string, int>>("TrimStringMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Switch selector - modify switching logic
        var switchStep = space.GetRequiredSwitchStep<string, int, string, int, int, int, int, int, int, int>("TestSwitchPipeline", "NumberRangeSwitch");
        var switchSelectorMutator = new StepMutator<SwitchSelector<string, int, int, int, int, int>>("NumberRangeSwitchMutator", 1, (selector) =>
        {
            return (input, cases, defaultNext) =>
            {
                if (int.TryParse(input, out var number))
                {
                    if (number > 50) // changed threshold
                    {
                        return cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        return cases["BetweenZeroAndHundred"](number);
                    }
                    else // number <= 0 goes to LessThanZero
                    {
                        return cases["LessThanZero"](number);
                    }
                }
                else
                {
                    var stringLength = input.Length;
                    return defaultNext(stringLength);
                }
            };
        });
        switchStep.Mutators.AddMutator(switchSelectorMutator, AddingMode.ExactPlace);

        // Case pipelines - pass through
        var multiplyByThreeStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("MultiplyByThree", "MultiplyOperation");
        var multiplyByThreeMutator = new StepMutator<Pipe<int, int, int, int>>("MultiplyByThreeMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        multiplyByThreeStep.Mutators.AddMutator(multiplyByThreeMutator, AddingMode.ExactPlace);

        var addTwoStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("AddTwo", "AddOperation");
        var addTwoMutator = new StepMutator<Pipe<int, int, int, int>>("AddTwoMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        addTwoStep.Mutators.AddMutator(addTwoMutator, AddingMode.ExactPlace);

        var multiplyByTwoStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("MultiplyByTwo", "MultiplyOperation");
        var multiplyByTwoMutator = new StepMutator<Pipe<int, int, int, int>>("MultiplyByTwoMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        multiplyByTwoStep.Mutators.AddMutator(multiplyByTwoMutator, AddingMode.ExactPlace);

        var keepZeroStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("KeepZero", "IdentityOperation");
        var keepZeroMutator = new StepMutator<Pipe<int, int, int, int>>("KeepZeroMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        keepZeroStep.Mutators.AddMutator(keepZeroMutator, AddingMode.ExactPlace);

        // Default pipeline - pass through
        var stringLengthStep = space.GetRequiredLinearStep<int, int, int, int, int, int>("StringLengthPipeline", "IdentityOperation");
        var stringLengthMutator = new StepMutator<Pipe<int, int, int, int>>("StringLengthMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        stringLengthStep.Mutators.AddMutator(stringLengthMutator, AddingMode.ExactPlace);

        // Handler - add 3
        var handlerStep = space.GetRequiredHandlerStep<string, int, int, int>("TestSwitchPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int, int>>("TestHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                input += 3;
                return handler(input);
            };
        });
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
        // Arrange
        var pipeline = Pipelines.CreateReturningSyncPipeline<string, (int?, string?)>("TestForkPipeline")
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
            // Digit processing branch
            space => space.CreatePipeline<string, (int?, string?)>("DigitProcessing")
                .StartWithLinear<string, (int?, string?)>("RemoveNonDigits", (input, next) =>
                {
                    var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                    return next(digitsOnly);
                })
                .ThenLinear<int, (int?, string?)>("ParseToInt", (input, next) =>
                {
                    if (int.TryParse(input, out var number))
                    {
                        return next(number);
                    }
                    else
                    {
                        return next(0);
                    }
                })
                .HandleWith("IntHandler", (input) => (input, null))
                .BuildPipeline(),
            // Non-digit processing branch
            space => space.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureForkPipelineMutators);
            });

        // Act
        var actualResult = pipeline(inputValue);

        // Assert
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
        // TrimString step - pass through
        var trimStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("TestForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("TrimStringMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Fork selector - modify logic: length > 3 -> digit branch; else -> non-digit branch
        var forkStep = space.GetRequiredForkStep<string, (int?, string?), string, (int?, string?), string, (int?, string?), string, (int?, string?)>("TestForkPipeline", "DigitContentFork");
        var forkSelectorMutator = new StepMutator<ForkSelector<string, (int?, string?), string, (int?, string?), string, (int?, string?)>>("DigitContentForkMutator", 1, (selector) =>
        {
            return (input, digitBranch, nonDigitBranch) =>
            {
                if (input.Length > 3)
                {
                    return digitBranch(input);
                }
                else
                {
                    return nonDigitBranch(input);
                }
            };
        });
        forkStep.Mutators.AddMutator(forkSelectorMutator, AddingMode.ExactPlace);

        // RemoveNonDigits - pass through
        var removeNonDigitsStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("DigitProcessing", "RemoveNonDigits");
        var removeNonDigitsMutator = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("RemoveNonDigitsMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        removeNonDigitsStep.Mutators.AddMutator(removeNonDigitsMutator, AddingMode.ExactPlace);

        // ParseToInt - add 5 to result
        var parseToIntStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), int, (int?, string?)>("DigitProcessing", "ParseToInt");
        var parseToIntMutator = new StepMutator<Pipe<string, (int?, string?), int, (int?, string?)>>("ParseToIntMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    return next(number + 5);
                }
                else
                {
                    return next(0 + 5);
                }
            };
        });
        parseToIntStep.Mutators.AddMutator(parseToIntMutator, AddingMode.ExactPlace);

        // RemoveDigits - pass through
        var removeDigitsStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "RemoveDigits");
        var removeDigitsMutator = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("RemoveDigitsMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        removeDigitsStep.Mutators.AddMutator(removeDigitsMutator, AddingMode.ExactPlace);

        // AddSpaces - use asterisks instead of spaces
        var addSpacesStep = space.GetRequiredLinearStep<string, (int?, string?), string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, (int?, string?), string, (int?, string?)>>("AddSpacesMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                var withAsterisks = $"***{input}***";
                return next(withAsterisks);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        // Handlers - pass through
        var intHandlerStep = space.GetRequiredHandlerStep<string, (int?, string?), int, (int?, string?)>("DigitProcessing", "IntHandler");
        var intHandlerMutator = new StepMutator<Handler<int, (int?, string?)>>("IntHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                return handler(input);
            };
        });
        intHandlerStep.Mutators.AddMutator(intHandlerMutator, AddingMode.ExactPlace);

        var stringHandlerStep = space.GetRequiredHandlerStep<string, (int?, string?), string, (int?, string?)>("NonDigitProcessing", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string, (int?, string?)>>("StringHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                return handler(input);
            };
        });
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
        var space = Pipelines.CreateReturningSyncSpace();

        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    return next(number);
                }
                else
                {
                    return next(0);
                }
            })
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", (input, next) =>
            {
                var result = input + 10;
                return next(result);
            })
            .HandleWith("IntHandler", (input) => (input, null, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", (input, next) =>
            {
                var withSpaces = $"  {input}  ";
                return next(withSpaces);
            })
            .HandleWith("StringHandler", (input) => (null, input, null))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("RemoveWhitespace", (input, next) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                return next(noWhitespace);
            })
            .ThenLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", (input, next) =>
            {
                var charArray = input.ToCharArray();
                return next(charArray);
            })
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", (input, next) =>
            {
                var uniqueChars = input.Distinct().ToArray();
                return next(uniqueChars);
            })
            .HandleWith("CharArrayHandler", (input) => (null, null, input))
            .BuildPipeline();

        var defaultPipeline = space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount), (int?, string?, char[]?)>("CountDigitsAndLetters", (input, next) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                return next((digitCount, letterCount));
            })
            .ThenLinear<int, (int?, string?, char[]?)>("CalculateRatio", (input, next) =>
            {
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                return next(ratio);
            })
            .HandleWith("IntHandler", (input) => (input, null, null))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                return next(trimmed);
            })
            .ThenMultiFork<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("ClassifyStringContent", (input, branches, defaultNext) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (containsOnlyDigits)
                {
                    return branches["DigitBranch"](input);
                }
                else if (containsOnlyLetters)
                {
                    return branches["LetterBranch"](input);
                }
                else if (containsOnlySpecialChars)
                {
                    return branches["SpecialCharBranch"](input);
                }
                else
                {
                    var charArray = input.ToCharArray();
                    return defaultNext(charArray);
                }
            },
            space => new Dictionary<string, Pipeline<string, (int?, string?, char[]?)>>
            {
                ["DigitBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")!,
                ["LetterBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            space => space.GetPipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")!)
            .BuildPipeline()
            .Compile(cfg => { cfg.Configure(ConfigureMultiForkPipelineMutators); });

        var actualResult = pipeline(inputValue);

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
        // TrimString - pass through
        var trimStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("TestMultiForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("TrimStringMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // MultiFork selector - treat everything except digits as special chars
        var multiForkStep = space.GetRequiredMultiForkStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("TestMultiForkPipeline", "ClassifyStringContent");
        var selectorMutator = new StepMutator<MultiForkSelector<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("ClassifyStringContentMutator", 1, (selector) =>
        {
            return (input, branches, defaultNext) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                if (containsOnlyDigits)
                {
                    return branches["DigitBranch"](input);
                }
                else
                {
                    return branches["SpecialCharBranch"](input);
                }
            };
        });
        multiForkStep.Mutators.AddMutator(selectorMutator, AddingMode.ExactPlace);

        // DigitProcessingPipeline - pass through ParseStringToInt
        var parseStringToIntStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "ParseStringToInt");
        var parseStringToIntMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>>("ParseStringToIntMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        parseStringToIntStep.Mutators.AddMutator(parseStringToIntMutator, AddingMode.ExactPlace);

        // DigitProcessingPipeline AddConstant - add +5 more
        var addConstantStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), int?, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int?, (int?, string?, char[]?), int?, (int?, string?, char[]?)>>("AddConstantMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                var result = (input ?? 0) + 10 + 5;
                return next(result);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // LetterProcessingPipeline AddSpaces - use asterisks
        var addSpacesStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("LetterProcessingPipeline", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("AddSpacesMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                var withAsterisks = $"***{input}***";
                return next(withAsterisks);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        // SpecialCharProcessingPipeline RemoveWhitespace - pass through
        var removeWhitespaceStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "RemoveWhitespace");
        var removeWhitespaceMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>>("RemoveWhitespaceMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        removeWhitespaceStep.Mutators.AddMutator(removeWhitespaceMutator, AddingMode.ExactPlace);

        // SpecialCharProcessingPipeline ConvertToCharArray - add underscore
        var convertToCharArrayStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "ConvertToCharArray");
        var convertToCharArrayMutator = new StepMutator<Pipe<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("ConvertToCharArrayMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                var inputWithUnderscore = input + "_";
                var charArray = inputWithUnderscore.ToCharArray();
                return next(charArray);
            };
        });
        convertToCharArrayStep.Mutators.AddMutator(convertToCharArrayMutator, AddingMode.ExactPlace);

        // SpecialCharProcessingPipeline RemoveDuplicates - pass through
        var removeDuplicatesStep = space.GetRequiredLinearStep<string, (int?, string?, char[]?), char[], (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "RemoveDuplicates");
        var removeDuplicatesMutator = new StepMutator<Pipe<char[], (int?, string?, char[]?), char[], (int?, string?, char[]?)>>("RemoveDuplicatesMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        removeDuplicatesStep.Mutators.AddMutator(removeDuplicatesMutator, AddingMode.ExactPlace);

        // DefaultProcessingPipeline CountDigitsAndLetters - pass through
        var countStep = space.GetRequiredLinearStep<char[], (int?, string?, char[]?), char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?)>("DefaultProcessingPipeline", "CountDigitsAndLetters");
        var countMutator = new StepMutator<Pipe<char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?)>>("CountDigitsAndLettersMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                return pipe(input, next);
            };
        });
        countStep.Mutators.AddMutator(countMutator, AddingMode.ExactPlace);

        // DefaultProcessingPipeline CalculateRatio - add +2
        var calculateRatioStep = space.GetRequiredLinearStep<char[], (int?, string?, char[]?), (int DigitCount, int LetterCount), (int?, string?, char[]?), int, (int?, string?, char[]?)>("DefaultProcessingPipeline", "CalculateRatio");
        var calculateRatioMutator = new StepMutator<Pipe<(int DigitCount, int LetterCount), (int?, string?, char[]?), int, (int?, string?, char[]?)>>("CalculateRatioMutator", 1, (pipe) =>
        {
            return (input, next) =>
            {
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                return next(ratio + 2);
            };
        });
        calculateRatioStep.Mutators.AddMutator(calculateRatioMutator, AddingMode.ExactPlace);

        // Handlers - pass through
        var intHandlerStepDigit = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), int?, (int?, string?, char[]?)>("DigitProcessingPipeline", "IntHandler");
        var intHandlerMutatorDigit = new StepMutator<Handler<int?, (int?, string?, char[]?)>>("IntHandlerMutatorDigit", 1, (handler) =>
        {
            return (input) =>
            {
                return handler(input);
            };
        });
        intHandlerStepDigit.Mutators.AddMutator(intHandlerMutatorDigit, AddingMode.ExactPlace);

        var stringHandlerStep = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), string, (int?, string?, char[]?)>("LetterProcessingPipeline", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string, (int?, string?, char[]?)>>("StringHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                return handler(input);
            };
        });
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);

        var charArrayHandlerStep = space.GetRequiredHandlerStep<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("SpecialCharProcessingPipeline", "CharArrayHandler");
        var charArrayHandlerMutator = new StepMutator<Handler<char[], (int?, string?, char[]?)>>("CharArrayHandlerMutator", 1, (handler) =>
        {
            return (input) =>
            {
                return handler(input);
            };
        });
        charArrayHandlerStep.Mutators.AddMutator(charArrayHandlerMutator, AddingMode.ExactPlace);

        var intHandlerStepDefault = space.GetRequiredHandlerStep<char[], (int?, string?, char[]?), int, (int?, string?, char[]?)>("DefaultProcessingPipeline", "IntHandler");
        var intHandlerMutatorDefault = new StepMutator<Handler<int, (int?, string?, char[]?)>>("IntHandlerMutatorDefault", 1, (handler) =>
        {
            return (input) =>
            {
                return handler(input);
            };
        });
        intHandlerStepDefault.Mutators.AddMutator(intHandlerMutatorDefault, AddingMode.ExactPlace);
    }
}

