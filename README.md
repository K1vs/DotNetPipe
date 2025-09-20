### DotNetPipe
A fluent-style pipeline builder with minimal overhead and a mutation API that lets you modify pipeline behavior without touching the original code. 
For full documentation, see the [DotNetPipe documentation site](https://k1vs.github.io/DotNetPipe).

### What it is for
Primarily aimed at library/framework authors who need to declare pipelines while allowing end users to adjust behavior. The author defines steps (linear and conditional) via a fluent API; clients can tweak the pipeline as needed. Invoking pipelines causes no extra allocations; the overhead is close to virtual method calls.

### Quick start (Universal, delegates)
Delegates are handy for quick scenarios and local transformations.

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
                    input *= 2;           // change behavior before the original step
                    await pipe(input, next);
                };
            });
            step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
        });
    });

await mutated(5); // the step will receive an already multiplied value
```

The mutator locates the step by name, casts it to `Pipe<int,int>`, and adds a wrapper without changing the original step code.

### Quick start (Universal, classes)
Classes are great for reuse, testing, and precise step naming.

```csharp
// 1) Classes: a simple linear pipeline
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

```csharp
// 2) Classes: a class-based mutator (IMutator<Space>)
public sealed class AddConstClassMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("DemoPipeline", "AddConst");
        var mutator = new StepMutator<Pipe<int, int>>("AddConst+5", 1, pipe =>
        {
            return async (input, next) => await pipe(input + 5, next);
        });
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

A class-based mutator is convenient to ship as an extension: the user just enables it in configuration, while you keep full control over the step’s behavior change.

### Mixing styles
You can mix delegates and classes within a single pipeline. The same applies to mutators.

```csharp
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Universal;
using K1vs.DotNetPipe.Mutations;

public sealed class AddConstStep2 : ILinearStep<int, int>
{
    public string Name => "AddConst";
    public async ValueTask Handle(int input, Handler<int> next)
    {
        await next(input + 10);
    }
}

public sealed class WriteHandler2 : IHandlerStep<int>
{
    public string Name => "Handler";
    public async ValueTask Handle(int input)
    {
        Console.WriteLine($"Result: {input}");
        await ValueTask.CompletedTask;
    }
}

// CLASS step + DELEGATE step + CLASS handler
var mixedPipeline = Pipelines.CreatePipeline<int>("MixedPipeline")
    .StartWithLinear(new AddConstStep2())
    .ThenLinear<int>("Multiply", async (input, next) => await next(input * 2))
    .HandleWith(new WriteHandler2())
    .BuildPipeline()
    .Compile(cfg =>
    {
        // Delegate mutator for a CLASS step
        cfg.Configure(space =>
        {
            var addConst = space.GetRequiredLinearStep<int, int, int>("MixedPipeline", "AddConst");
            addConst.Mutators.AddMutator(
                new StepMutator<Pipe<int, int>>("AddConst+5", 1, pipe => async (input, next) => await pipe(input + 5, next)),
                AddingMode.ExactPlace);
        });

        // CLASS mutator for a DELEGATE step
        cfg.Configure(new IMutator<Space>[] { new MultiplyStepMutator() });
    });

await mixedPipeline(5); // (5 + 10 + 5) * 2 -> prints: Result: 40

public sealed class MultiplyStepMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var multiply = space.GetRequiredLinearStep<int, int, int>("MixedPipeline", "Multiply");
        multiply.Mutators.AddMutator(
            new StepMutator<Pipe<int, int>>("Multiply*3", 1, pipe => async (input, next) => await pipe(input * 3, next)),
            AddingMode.ExactPlace);
    }
}
```

### Feature overview
- **if**: a conditional branch that runs only when the condition is true (otherwise continue the main pipeline).
- **ifelse**: two alternative branches (true/false) followed by a merge back into the main flow.
- **switch**: a set of named branches selected by a selector; includes a default branch.
- **fork**: split into two branches with independent sub-pipelines.
- **multifork**: split into an arbitrary number of branches (name → sub-pipeline map) with a default branch.

The library provides three pipeline kinds in terms of asynchrony: **Universal** (returns `ValueTask`), **Async** (returns `Task`), and **Sync**. Each has a variant that accepts a cancellation token. While pipelines typically do not return a value, returning variants are available for cases when you need a result (in all of the above modes).

### Mutators concept
Mutators modify step behavior by locating a step by name, casting it to the expected type, and adding a wrapping function. This enables substantial behavior changes or extensions while respecting already-added mutators and their order/priority.

### Note and license
This is “syntactic sugar” for declaring pipelines: it limits capabilities but reduces the chance of bugs and performance penalties. Distributed under the MIT License, “as is”, without warranties or liability. Feedback and contributions are welcome via issues/PRs.
