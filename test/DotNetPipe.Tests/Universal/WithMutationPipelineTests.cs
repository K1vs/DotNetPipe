using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Universal;
using NSubstitute;

namespace K1vs.DotNetPipe.Tests.Universal;

public class WithMutationPipelineTests
{
    [Theory]
    [InlineData(-4, -3)]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStep_ShouldRun(int value, int expectedValue)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(value)).Returns(ValueTask.CompletedTask);
        var pipeline = Pipelines.CreatePipeline<int>("TestPipeline")
            .StartWithHandler("TestHandler", async (input) => await handler(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureTestPipelineMutators);
            });
        // Act
        await pipeline(value);
        // Assert
        await handler.Received().Invoke(Arg.Is(expectedValue));
    }

    private void ConfigureTestPipelineMutators(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int>("TestPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(-4, 5, 3)]  // (-4 + 5) * 2 + 1 = 3
    [InlineData(0, 10, 21)]  // (0 + 10) * 2 + 1 = 21
    [InlineData(2, 3, 11)]   // (2 + 3) * 2 + 1 = 11
    public async Task BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedHandlerInput)).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreatePipeline<int>("TestTwoStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureTwoStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput));
    }

    private void ConfigureTwoStepPipelineMutators(Space space)
    {
        // Mutator for linear step - multiply by 2
        var linearStep = space.GetRequiredLinearStep<int, int, int>("TestTwoStepPipeline", "AddConstant");
        var linearMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply original linear step logic first
                await pipe(input, async (result) =>
                {
                    // Then apply mutation - multiply by 2
                    result *= 2;
                    await next(result);
                });
            };
        });
        linearStep.Mutators.AddMutator(linearMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1
        var handlerStep = space.GetRequiredHandlerStep<int, int>("TestTwoStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(2, 3, 2, 71)]   // Step1: (2 + 3) + 5 = 10, Step2: 10 * 2 + 10 * 5 = 70, Handler: 70 + 1 = 71
    [InlineData(0, 5, 3, 81)]   // Step1: (0 + 5) + 5 = 10, Step2: 10 * 3 + 10 * 5 = 80, Handler: 80 + 1 = 81
    [InlineData(-1, 4, 2, 57)]  // Step1: (-1 + 4) + 5 = 8, Step2: 8 * 2 + 8 * 5 = 56, Handler: 56 + 1 = 57
    [InlineData(10, -5, 4, 91)] // Step1: (10 + (-5)) + 5 = 10, Step2: 10 * 4 + 10 * 5 = 90, Handler: 90 + 1 = 91
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int multiplier, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedHandlerInput)).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreatePipeline<int>("TestThreeStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .ThenLinear<int>("MultiplyByCoefficient", async (input, next) =>
            {
                var result = input * multiplier;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureThreeStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput));
    }

    private void ConfigureThreeStepPipelineMutators(Space space)
    {
        // Mutator for first linear step (AddConstant) - add 5 to result
        var firstLinearStep = space.GetRequiredLinearStep<int, int, int>("TestThreeStepPipeline", "AddConstant");
        var firstLinearMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply original linear step logic first
                await pipe(input, async (result) =>
                {
                    // Then apply mutation - add 5
                    result += 5;
                    await next(result);
                });
            };
        });
        firstLinearStep.Mutators.AddMutator(firstLinearMutator, AddingMode.ExactPlace);

        // Mutator for second linear step (MultiplyByCoefficient) - add 5 to multiplier
        var secondLinearStep = space.GetRequiredLinearStep<int, int, int>("TestThreeStepPipeline", "MultiplyByCoefficient");
        var secondLinearMutator = new StepMutator<Pipe<int, int>>("MultiplyByCoefficient", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply mutation to input first - simulate changing the multiplier by adding 5 to input
                // Since we can't change the multiplier directly, we'll modify the calculation logic
                await pipe(input, async (result) =>
                {
                    // Recalculate: instead of input * multiplier, do input * (multiplier + 5)
                    // But we already got result = input * multiplier, so we need to adjust
                    // result = input * multiplier, so multiplier = result / input (if input != 0)
                    // new_result = input * (multiplier + 5) = input * multiplier + input * 5 = result + input * 5
                    if (input != 0)
                    {
                        result = result + input * 5; // This gives us input * (multiplier + 5)
                    }
                    await next(result);
                });
            };
        });
        secondLinearStep.Mutators.AddMutator(secondLinearMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1
        var handlerStep = space.GetRequiredHandlerStep<int, int>("TestThreeStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("5", 3, 11)]    // "5" -> 5 + 2 = 7, 7 + 3 = 10, handler gets 10 + 1 = 11
    [InlineData("10", -2, 11)]  // "10" -> 10 + 2 = 12, 12 + (-2) = 10, handler gets 10 + 1 = 11
    [InlineData("0", 5, 8)]     // "0" -> 0 + 2 = 2, 2 + 5 = 7, handler gets 7 + 1 = 8
    [InlineData("abc", 3, 0)]   // "abc" -> doesn't parse, handler not called
    [InlineData("", 10, 0)]     // "" -> doesn't parse, handler not called
    public async Task BuildAndRunPipeline_WhenStringParseAndAddConstant_ShouldCallHandlerOnlyOnSuccessfulParse(string inputValue, int constantToAdd, int expectedCallCount)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Any<int>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreatePipeline<string>("TestStringParsingPipeline")
            .StartWithLinear<int>("ParseString", async (input, next) =>
            {
                if (int.TryParse(input, out var parsed))
                {
                    await next(parsed);
                }
                // If doesn't parse - don't call next
            })
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureStringParsingPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        if (expectedCallCount > 0)
        {
            await handler.Received().Invoke(Arg.Is(expectedCallCount));
        }
        else
        {
            await handler.DidNotReceive().Invoke(Arg.Any<int>());
        }
    }

    private void ConfigureStringParsingPipelineMutators(Space space)
    {
        // Mutator for ParseString step - add 2 to parsed value (only if parsing succeeds)
        var parseStep = space.GetRequiredLinearStep<string, string, int>("TestStringParsingPipeline", "ParseString");
        var parseMutator = new StepMutator<Pipe<string, int>>("ParseStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply original parsing logic first
                await pipe(input, async (parsed) =>
                {
                    // Then apply mutation - add 2 to successfully parsed value
                    parsed += 2;
                    await next(parsed);
                });
            };
        });
        parseStep.Mutators.AddMutator(parseMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step - no additional changes, just pass through
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("TestStringParsingPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply original add constant logic as-is
                await pipe(input, next);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestStringParsingPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(" 10 ", -2, 8)]     // " 10 " -> trim -> 10 (int) -> 10 + (-2) = 8 (handler adds 1 but this is int path, so no mutation)
    [InlineData("3.7", 5, 10)]      // "3.7" -> trim -> 3.7 (float) -> round to 4 + 1 (mutator) = 5 -> 5 + 5 = 10
    [InlineData(" 2.3 ", 1, 4)]     // " 2.3 " -> trim -> 2.3 (float) -> round to 2 + 1 (mutator) = 3 -> 3 + 1 = 4
    [InlineData("5.5", 2, 9)]       // "5.5" -> trim -> 5.5 (float) -> round to 6 + 1 (mutator) = 7 -> 7 + 2 = 9
    public async Task BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int constantToAdd, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult)).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreatePipeline<string>("TestIfStepPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenIf<string, int>("CheckIntOrFloat", async (input, conditionalNext, next) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, continue with main pipeline
                    await next(intValue);
                }
                else
                {
                    // If not an int, go to conditional pipeline (for float parsing)
                    await conditionalNext(input);
                }
            }, space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", async (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        await next(floatValue);
                    }
                })
                .ThenLinear<int>("RoundToInt", async (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    await next(rounded);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureIfStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult));
    }

    private void ConfigureIfStepPipelineMutators(Space space)
    {
        // Mutator for TrimString step - no change needed, just pass through
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestIfStepPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for the RoundToInt step in the conditional pipeline - add 1 to the rounded value
        var roundStep = space.GetRequiredLinearStep<string, double, int>("FloatProcessing", "RoundToInt");
        var roundMutator = new StepMutator<Pipe<double, int>>("RoundToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, async (rounded) =>
                {
                    // Add 1 to the rounded value
                    rounded += 1;
                    await next(rounded);
                });
            };
        });
        roundStep.Mutators.AddMutator(roundMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step - no additional changes
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("TestIfStepPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for handler step - no change for this test
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestIfStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("  5  ", 3, 2, 16)]    // "  5  " -> trim -> 5 (int) -> false branch -> 5 * 2 = 10 -> +2 = 12 -> 12 + 3 = 15 -> +1 = 16
    [InlineData(" 10 ", -2, 4, 41)]    // " 10 " -> trim -> 10 (int) -> false branch -> 10 * 4 = 40 -> +2 = 42 -> 42 + (-2) = 40 -> +1 = 41
    [InlineData("3.7", 5, 3, 11)]      // "3.7" -> trim -> 3.7 (float) -> true branch -> round to 4 -> +1 = 5 -> 5 + 5 = 10 -> +1 = 11
    [InlineData(" 2.3 ", 1, 5, 5)]     // " 2.3 " -> trim -> 2.3 (float) -> true branch -> round to 2 -> +1 = 3 -> 3 + 1 = 4 -> +1 = 5
    [InlineData("5.5", 2, 7, 10)]      // "5.5" -> trim -> 5.5 (float) -> true branch -> round to 6 -> +1 = 7 -> 7 + 2 = 9 -> +1 = 10
    public async Task BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult)).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreatePipeline<string>("TestIfElseStepPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenIfElse<string, int, int>("CheckIntOrFloat", async (input, trueNext, falseNext) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, we actually want to bypass both branches and continue directly
                    // But since IfElse requires going through one of the branches, we'll use false branch for int
                    await falseNext(intValue);
                }
                else
                {
                    // If not an int, go to true branch (for float parsing)
                    await trueNext(input);
                }
            },
            // True branch - float processing
            space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", async (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        await next(floatValue);
                    }
                })
                .ThenLinear<int>("RoundToInt", async (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    await next(rounded);
                })
                .BuildOpenPipeline(),
            // False branch - multiply by multiplier
            space => space.CreatePipeline<int>("IntOrDefaultProcessing")
                .StartWithLinear<int>("ParseIntOrDefault", async (input, next) =>
                {
                    // If we got here, it means we parsed as int in the false branch
                    // So we just pass it through
                    await next(input * multiplier);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureIfElseStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult));
    }

    private void ConfigureIfElseStepPipelineMutators(Space space)
    {
        // Mutator for TrimString step - no change needed, just pass through
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestIfElseStepPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for the RoundToInt step in the FloatProcessing pipeline - add 1 to the rounded value
        var roundStep = space.GetRequiredLinearStep<string, double, int>("FloatProcessing", "RoundToInt");
        var roundMutator = new StepMutator<Pipe<double, int>>("RoundToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, async (rounded) =>
                {
                    // Add 1 to the rounded value
                    rounded += 1;
                    await next(rounded);
                });
            };
        });
        roundStep.Mutators.AddMutator(roundMutator, AddingMode.ExactPlace);        // Mutator for ParseIntOrDefault step in the IntOrDefaultProcessing pipeline - add 2 to the result
        var parseIntStep = space.GetRequiredLinearStep<int, int, int>("IntOrDefaultProcessing", "ParseIntOrDefault");
        var parseIntMutator = new StepMutator<Pipe<int, int>>("ParseIntOrDefaultMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, async (result) =>
                {
                    // Add 2 to the multiplied result
                    result += 2;
                    await next(result);
                });
            };
        });
        parseIntStep.Mutators.AddMutator(parseIntMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step - no additional changes
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("TestIfElseStepPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1 to the final result
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestIfElseStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                // Add 1 to the final result before calling the handler
                input += 1;
                await handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(" 105 ", 318)]    // >100 -> *3 = 315 -> +3 = 318
    [InlineData(" 50 ", 55)]      // 0<x<100 -> +2 = 52 -> +3 = 55
    [InlineData(" -5 ", -7)]      // <0 -> *2 = -10 -> +3 = -7
    [InlineData(" 0 ", 3)]        // =0 -> stay 0 -> +3 = 3
    [InlineData("abc", 6)]      // not a number -> string length = 3 -> +3 = 6
    [InlineData("hello", 8)]    // not a number -> string length = 5 -> +3 = 8
    [InlineData("", 3)]         // empty string -> length = 0 -> +3 = 3
    public async Task BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult)).Returns(ValueTask.CompletedTask);

        var space = new Space();
        var defaultPipeline = space.CreatePipeline<int>("StringLengthPipeline")
            .StartWithLinear<int>("IdentityOperation", async (input, next) =>
            {
                await next(input); // Use string length as-is
            })
            .BuildOpenPipeline();

        var pipeline = Pipelines.CreatePipeline<string>("TestSwitchPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenSwitch<int, int, int>("NumberRangeSwitch", async (input, cases, defaultNext) =>
            {
                // Try to parse as integer
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        await cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        await cases["BetweenZeroAndHundred"](number);
                    }
                    else if (number < 0)
                    {
                        await cases["LessThanZero"](number);
                    }
                    else // number == 0
                    {
                        await cases["EqualToZero"](number);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    await defaultNext(stringLength);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int>("MultiplyByThree")
                    .StartWithLinear<int>("MultiplyOperation", async (input, next) =>
                    {
                        var result = input * 3;
                        await next(result);
                    })
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int>("AddTwo")
                    .StartWithLinear<int>("AddOperation", async (input, next) =>
                    {
                        var result = input + 2;
                        await next(result);
                    })
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int>("MultiplyByTwo")
                    .StartWithLinear<int>("MultiplyOperation", async (input, next) =>
                    {
                        var result = input * 2;
                        await next(result);
                    })
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int>("KeepZero")
                    .StartWithLinear<int>("IdentityOperation", async (input, next) =>
                    {
                        await next(input); // Keep the same value (0)
                    })
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureSwitchStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult));
    }

    private void ConfigureSwitchStepPipelineMutators(Space space)
    {
        // Mutator for TrimString step - no change needed, just pass through
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestSwitchPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 3 to all results
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestSwitchPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 3;
                await handler(input);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("123", 128, "")]             // Only digits -> first branch -> parse to int (123) + 5 = 128
    [InlineData("  456  ", 461, "")]         // Only digits (with spaces) -> first branch -> 456 + 5 = 461
    [InlineData("abc123def", 0, "***abcdef***")] // Mixed -> second branch -> remove digits -> add asterisks instead of spaces
    [InlineData("hello", 0, "***hello***")]    // No digits -> second branch -> add asterisks
    [InlineData("", 0, "******")]              // Empty -> second branch -> add asterisks
    [InlineData("!@#", 0, "***!@#***")]        // Special chars -> second branch -> add asterisks
    public async Task BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int expectedIntResult, string? expectedStringResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, ValueTask>>();
        var stringHandler = Substitute.For<Func<string, ValueTask>>();
        intHandler.Invoke(Arg.Any<int>()).Returns(ValueTask.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreatePipeline<string>("TestForkPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenFork<string, string>("DigitContentFork", async (input, digitBranch, nonDigitBranch) =>
            {
                // Check if string contains only digits (after trimming)
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    await digitBranch(input);
                }
                else
                {
                    await nonDigitBranch(input);
                }
            },
            // Digit processing branch
            space => space.CreatePipeline<string>("DigitProcessing")
                .StartWithLinear<string>("RemoveNonDigits", async (input, next) =>
                {
                    var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                    await next(digitsOnly);
                })
                .ThenLinear<int>("ParseToInt", async (input, next) =>
                {
                    if (int.TryParse(input, out var number))
                    {
                        await next(number);
                    }
                    else
                    {
                        await next(0); // Default to 0 if parsing fails
                    }
                })
                .HandleWith("IntHandler", async (input) => await intHandler(input))
                .BuildPipeline(),
            // Non-digit processing branch
            space => space.CreatePipeline<string>("NonDigitProcessing")
                .StartWithLinear<string>("RemoveDigits", async (input, next) =>
                {
                    var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                    await next(nonDigitsOnly);
                })
                .ThenLinear<string>("AddSpaces", async (input, next) =>
                {
                    var withSpaces = $"  {input}  ";
                    await next(withSpaces);
                })
                .HandleWith("StringHandler", async (input) => await stringHandler(input))
                .BuildPipeline())
            .BuildPipeline()            .Compile(cfg =>
            {
                cfg.Configure(ConfigureForkPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        if (expectedStringResult == "")
        {
            // Should call int handler (digit-only string)
            await intHandler.Received().Invoke(Arg.Is(expectedIntResult));
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
        }
        else
        {
            // Should call string handler (non-digit string)
            await stringHandler.Received().Invoke(Arg.Is(expectedStringResult!));
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>());
        }
    }

    private void ConfigureForkPipelineMutators(Space space)
    {
        // Mutator for TrimString step - no change needed, just pass through
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for ParseToInt step in the DigitProcessing pipeline - add 5 to the parsed value
        var parseToIntStep = space.GetRequiredLinearStep<string, string, int>("DigitProcessing", "ParseToInt");
        var parseToIntMutator = new StepMutator<Pipe<string, int>>("ParseToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    await next(number + 5); // Add 5 to the parsed value
                }
                else
                {
                    await next(0);
                }
            };
        });
        parseToIntStep.Mutators.AddMutator(parseToIntMutator, AddingMode.ExactPlace);

        // Mutator for AddSpaces step in the NonDigitProcessing pipeline - use asterisks instead of spaces
        var addSpacesStep = space.GetRequiredLinearStep<string, string, string>("NonDigitProcessing", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, string>>("AddSpacesMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
                await next(withAsterisks);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        // Mutator for IntHandler - no additional changes
        var intHandlerStep = space.GetRequiredHandlerStep<string, int>("DigitProcessing", "IntHandler");
        var intHandlerMutator = new StepMutator<Handler<int>>("IntHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        intHandlerStep.Mutators.AddMutator(intHandlerMutator, AddingMode.ExactPlace);

        // Mutator for StringHandler - no additional changes
        var stringHandlerStep = space.GetRequiredHandlerStep<string, string>("NonDigitProcessing", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string>>("StringHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("123", 138, "", new char[0])]             // Only digits -> first branch -> parse to int (123) + 10 + 5 = 138
    [InlineData("  456  ", 471, "", new char[0])]         // Only digits (with spaces) -> first branch -> 456 + 10 + 5 = 471
    [InlineData("abc", 0, "***abc***", new char[0])]      // Only letters -> second branch -> add asterisks instead of spaces
    [InlineData("xyz", 0, "***xyz***", new char[0])]      // Only letters -> second branch -> add asterisks instead of spaces
    [InlineData("!@#", 0, "", new char[] { '!', '@', '#', '_' })] // Only special chars -> third branch -> add underscore, remove whitespace, convert to array, remove duplicates
    [InlineData("@@@", 0, "", new char[] { '@', '_' })]   // Special chars with duplicates -> third branch -> unique chars + underscore
    [InlineData("a1b2", 3, "", new char[0])]              // Mixed -> default branch -> 2 digits, 2 letters -> ratio = 1 + 2 = 3
    [InlineData("hello123", 2, "", new char[0])]          // Mixed -> default branch -> 3 digits, 5 letters -> ratio = 0 + 2 = 2
    [InlineData("12345abc", 3, "", new char[0])]          // Mixed -> default branch -> 5 digits, 3 letters -> ratio = 1 + 2 = 3
    public async Task BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int expectedIntResult,
        string? expectedStringResult,
        char[] expectedCharArrayResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, ValueTask>>();
        var stringHandler = Substitute.For<Func<string, ValueTask>>();
        var charArrayHandler = Substitute.For<Func<char[], ValueTask>>();

        intHandler.Invoke(Arg.Any<int>()).Returns(ValueTask.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>()).Returns(ValueTask.CompletedTask);
        charArrayHandler.Invoke(Arg.Any<char[]>()).Returns(ValueTask.CompletedTask);

        var space = Pipelines.CreateSpace();

        // Create sub-pipelines before main pipeline
        space.CreatePipeline<string>("DigitProcessingPipeline")
            .StartWithLinear<int>("ParseStringToInt", async (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    await next(number);
                }
                else
                {
                    await next(0); // Default value if parsing fails
                }
            })
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + 10; // Add constant 10
                await next(result);
            })
            .HandleWith("IntHandler", async (input) => await intHandler(input))
            .BuildPipeline();

        space.CreatePipeline<string>("LetterProcessingPipeline")
            .StartWithLinear<string>("AddSpaces", async (input, next) =>
            {
                var withSpaces = $"  {input}  ";
                await next(withSpaces);
            })
            .HandleWith("StringHandler", async (input) => await stringHandler(input))
            .BuildPipeline();

        space.CreatePipeline<string>("SpecialCharProcessingPipeline")
            .StartWithLinear<string>("RemoveWhitespace", async (input, next) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                await next(noWhitespace);
            })
            .ThenLinear<char[]>("ConvertToCharArray", async (input, next) =>
            {
                var charArray = input.ToCharArray();
                await next(charArray);
            })
            .ThenLinear<char[]>("RemoveDuplicates", async (input, next) =>
            {
                var uniqueChars = input.Distinct().ToArray();
                await next(uniqueChars);
            })
            .HandleWith("CharArrayHandler", async (input) => await charArrayHandler(input))
            .BuildPipeline();

        var defaultPipeline = space.CreatePipeline<char[]>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount)>("CountDigitsAndLetters", async (input, next) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                await next((digitCount, letterCount));
            })
            .ThenLinear<int>("CalculateRatio", async (input, next) =>
            {
                // Calculate ratio of digits to letters (floor division)
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                await next(ratio);
            })
            .HandleWith("IntHandler", async (input) => await intHandler(input))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string>("TestMultiForkPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenMultiFork<string, char[]>("ClassifyStringContent", async (input, branches, defaultNext) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (containsOnlyDigits)
                {
                    await branches["DigitBranch"](input);
                }
                else if (containsOnlyLetters)
                {
                    await branches["LetterBranch"](input);
                }
                else if (containsOnlySpecialChars)
                {
                    await branches["SpecialCharBranch"](input);
                }
                else
                {
                    // Mixed content - go to default branch
                    var charArray = input.ToCharArray();
                    await defaultNext(charArray);
                }
            },
            space => new Dictionary<string, Pipeline<string>>
            {
                ["DigitBranch"] = space.GetPipeline<string>("DigitProcessingPipeline")!,
                ["LetterBranch"] = space.GetPipeline<string>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = space.GetPipeline<string>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            space => space.GetPipeline<char[]>("DefaultProcessingPipeline")!)
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureMultiForkPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        if (expectedStringResult != "")
        {
            // Should call string handler (letter-only string)
            await stringHandler.Received().Invoke(Arg.Is(expectedStringResult!));
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>());
        }
        else if (expectedCharArrayResult.Length > 0)
        {
            // Should call char array handler (special char-only string)
            await charArrayHandler.Received().Invoke(Arg.Is<char[]>(arr => arr.SequenceEqual(expectedCharArrayResult)));
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>());
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
        }
        else
        {
            // Should call int handler (digit-only string or mixed content)
            await intHandler.Received().Invoke(Arg.Is(expectedIntResult));
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>());
        }
    }

    private void ConfigureMultiForkPipelineMutators(Space space)
    {
        // Mutator for TrimString step - no change needed, just pass through
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestMultiForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step in DigitProcessingPipeline - add 5 more to the result
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("DigitProcessingPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var result = input + 10 + 5; // Original +10, mutation adds +5 more
                await next(result);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for AddSpaces step in LetterProcessingPipeline - use asterisks instead of spaces
        var addSpacesStep = space.GetRequiredLinearStep<string, string, string>("LetterProcessingPipeline", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, string>>("AddSpacesMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
                await next(withAsterisks);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        // Mutator for ConvertToCharArray step in SpecialCharProcessingPipeline - add underscore
        var convertToCharArrayStep = space.GetRequiredLinearStep<string, string, char[]>("SpecialCharProcessingPipeline", "ConvertToCharArray");
        var convertToCharArrayMutator = new StepMutator<Pipe<string, char[]>>("ConvertToCharArrayMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var inputWithUnderscore = input + "_"; // Add underscore before converting
                var charArray = inputWithUnderscore.ToCharArray();
                await next(charArray);
            };
        });
        convertToCharArrayStep.Mutators.AddMutator(convertToCharArrayMutator, AddingMode.ExactPlace);

        // Mutator for CalculateRatio step in DefaultProcessingPipeline - add 2 to the ratio
        var calculateRatioStep = space.GetRequiredLinearStep<char[], (int DigitCount, int LetterCount), int>("DefaultProcessingPipeline", "CalculateRatio");
        var calculateRatioMutator = new StepMutator<Pipe<(int DigitCount, int LetterCount), int>>("CalculateRatioMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Calculate ratio of digits to letters (floor division) and add 2
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                await next(ratio + 2); // Add 2 to the calculated ratio
            };
        });
        calculateRatioStep.Mutators.AddMutator(calculateRatioMutator, AddingMode.ExactPlace);

        // Mutators for handlers - no additional changes needed
        var intHandlerStepDigit = space.GetRequiredHandlerStep<string, int>("DigitProcessingPipeline", "IntHandler");
        var intHandlerMutatorDigit = new StepMutator<Handler<int>>("IntHandlerMutatorDigit", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        intHandlerStepDigit.Mutators.AddMutator(intHandlerMutatorDigit, AddingMode.ExactPlace);

        var stringHandlerStep = space.GetRequiredHandlerStep<string, string>("LetterProcessingPipeline", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string>>("StringHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);

        var charArrayHandlerStep = space.GetRequiredHandlerStep<string, char[]>("SpecialCharProcessingPipeline", "CharArrayHandler");
        var charArrayHandlerMutator = new StepMutator<Handler<char[]>>("CharArrayHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        charArrayHandlerStep.Mutators.AddMutator(charArrayHandlerMutator, AddingMode.ExactPlace);

        var intHandlerStepDefault = space.GetRequiredHandlerStep<char[], int>("DefaultProcessingPipeline", "IntHandler");
        var intHandlerMutatorDefault = new StepMutator<Handler<int>>("IntHandlerMutatorDefault", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        intHandlerStepDefault.Mutators.AddMutator(intHandlerMutatorDefault, AddingMode.ExactPlace);
    }
}