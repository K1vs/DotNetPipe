---
_layout: landing
---

# DotNetPipe

DotNetPipe is a fluent-style pipeline builder with minimal overhead and a mutation API that lets you modify pipeline behavior without touching the original code.

- Minimal allocations at runtime; overhead close to virtual method calls
- Fluent API for linear and conditional steps
- Mutation API to alter steps post-factum (per step, by name and type)
- Multiple pipeline kinds: Universal (ValueTask), Async (Task), Sync; each has cancellable and returning variants

Use the links below to dive in:

- [Getting Started](articles/getting-started.md)
- [Introduction](articles/introduction.md)

Repository: [GitHub - K1vs/DotNetPipe](https://github.com/K1vs/DotNetPipe)