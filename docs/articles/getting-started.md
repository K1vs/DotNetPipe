# Getting Started

This guide walks through four small, composable examples, with brief explanations between code blocks. They use Universal pipelines (ValueTask) and the same step/mutator patterns used elsewhere in this library.

```csharp
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Universal;
using K1vs.DotNetPipe.Mutations;

// 1) Delegates: a simple linear pipeline
var pipeline = Pipelines.CreatePipeline<int>("DemoPipeline")
    .StartWithLinear<int>("AddConst", async (input, next) =>
    {
        var result = input + 10;
        await next(result);
    })
    .HandleWith("Handler", async value =>
    {
        Console.WriteLine($"Result: {value}");
        await ValueTask.CompletedTask;
    })
    .BuildPipeline()
    .Compile();

await pipeline(5); // prints: Result: 15
```

The first example defines a linear step that adds a constant and a final handler. No extra allocations at runtime.

```csharp
// 2) Delegates: add a mutator via cfg.Configure (modify the "AddConst" step)
var mutated = Pipelines.CreatePipeline<int>("DemoPipeline")
    .StartWithLinear<int>("AddConst", async (input, next) => await next(input + 10))
    .HandleWith("Handler", async _ => await ValueTask.CompletedTask)
    .BuildPipeline()
    .Compile(cfg =>
    {
        cfg.Configure(space =>
        {
            var step = space.GetRequiredLinearStep<int, int, int>("DemoPipeline", "AddConst");
            var mutator = new StepMutator<Pipe<int, int>>("AddConst*2", 1, pipe =>
            {
                return async (input, next) =>
                {
                    input *= 2;
                    await pipe(input, next);
                };
            });
            step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
        });
    });

await mutated(5); // the step will receive an already multiplied value
```

The mutator locates the step by name, casts it to `Pipe<int,int>`, and adds a wrapper without changing the original step code.

```csharp
// 3) Classes: a simple linear pipeline
public sealed class AddConstStep : ILinearStep<int, int>
{
    public string Name => "AddConst";
    public async ValueTask Handle(int input, Handler<int> next)
    {
        await next(input + 10);
    }
}

public sealed class WriteHandler : IHandlerStep<int>
{
    public string Name => "Handler";
    public async ValueTask Handle(int input)
    {
        Console.WriteLine($"Result: {input}");
        await ValueTask.CompletedTask;
    }
}

var classPipeline = Pipelines.CreatePipeline<int>("DemoPipeline")
    .StartWithLinear(new AddConstStep())
    .HandleWith(new WriteHandler())
    .BuildPipeline()
    .Compile();

await classPipeline(5);
```

Class-based steps provide better readability and precise addressing by name for mutators.

```csharp
// 4) Classes: a class-based mutator (IMutator<Space>)
public sealed class AddConstClassMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("DemoPipeline", "AddConst");
        var mutator = new StepMutator<Pipe<int, int>>("AddConst+5", 1, pipe => async (input, next) => await pipe(input + 5, next));
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

var classMutated = Pipelines.CreatePipeline<int>("DemoPipeline")
    .StartWithLinear(new AddConstStep())
    .HandleWith(new WriteHandler())
    .BuildPipeline()
    .Compile(cfg =>
    {
        cfg.Configure(new IMutator<Space>[] { new AddConstClassMutator() });
    });

await classMutated(5); // result: (5 + 5) + 10
```

You can mix delegates and classes in a single pipeline, and apply both delegate- and class-based mutators.