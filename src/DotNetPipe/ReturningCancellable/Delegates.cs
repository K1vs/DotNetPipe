using System.Threading;

namespace K1vs.DotNetPipe.ReturningCancellable;

/// <summary>
/// Delegate for a selector that takes an input and two handlers, one for the "if" case and one for the "next" case.
/// Used in conditional logic within a returning pipeline with cancellation support.
/// After true handler is executed, the next handler is executed or execution is finished.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the selector.</typeparam>
/// <typeparam name="TIfInput">The type of the input data for the "if" case.</typeparam>
/// <typeparam name="TIfResult">The type of the result produced by the "if" handler.</typeparam>
/// <typeparam name="TNextInput">The type of the input data for the "next" case.</typeparam>
/// <typeparam name="TNextResult">The type of the result produced by the "next" handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="ifNext">The handler to execute if decide in selector.</param>
/// <param name="next">The handler to execute after the "if" handler is executed.</param>
/// <param name="ct">The cancellation token to observe.</param>
/// <returns>A value task representing the asynchronous operation.</returns>
public delegate ValueTask<TResult> IfSelector<TInput, TResult, TIfInput, TIfResult, TNextInput, TNextResult>(
    TInput input,
    Handler<TIfInput, TIfResult> ifNext,
    Handler<TNextInput, TNextResult> next,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a selector that takes an input and two handlers, one for the "if" case and one for the "else" case.
/// Next handler is executed after the "if" or "else" handler is executed.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the selector.</typeparam>
/// <typeparam name="TIfInput">The type of the input data for the "if" case.</typeparam>
/// <typeparam name="TIfResult">The type of the result produced by the "if" handler.</typeparam>
/// <typeparam name="TElseInput">The type of the input data for the "else" case.</typeparam>
/// <typeparam name="TElseResult">The type of the result produced by the "else" handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="ifNext">The handler to execute if decide in selector.</param>
/// <param name="elseNext">The handler to execute if decide in selector.</param>
/// <param name="ct">The cancellation token to observe.</param>
/// <returns>A value task representing the asynchronous operation.</returns>
public delegate ValueTask<TResult> IfElseSelector<TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult>(
    TInput input,
    Handler<TIfInput, TIfResult> ifNext,
    Handler<TElseInput, TElseResult> elseNext,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a selector that takes an input, a dictionary of cases, and a default handler.
/// This is used in switch-case logic within a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the selector.</typeparam>
/// <typeparam name="TCaseInput">The type of the input data for the case handlers.</typeparam>
/// <typeparam name="TCaseResult">The type of the result produced by the case handlers.</typeparam>
/// <typeparam name="TDefaultInput">The type of the input data for the default handler.</typeparam>
/// <typeparam name="TDefaultResult">The type of the result produced by the default handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="cases">The dictionary of case handlers.</param>
/// <param name="defaultNext">The default handler to execute if no case matches.</param>
/// <param name="ct">The cancellation token to observe.</param>
/// <returns>A value task representing the asynchronous operation.</returns>
public delegate ValueTask<TResult> SwitchSelector<TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult>(
    TInput input,
    IReadOnlyDictionary<string, Handler<TCaseInput, TCaseResult>> cases,
    Handler<TDefaultInput, TDefaultResult> defaultNext,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a fork selector that takes an input and two branch handlers.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the selector.</typeparam>
/// <typeparam name="TBranchAInput">The type of the input data for the first branch.</typeparam>
/// <typeparam name="TResultA">The type of the result produced by the first branch handler.</typeparam>
/// <typeparam name="TBranchBInput">The type of the input data for the second branch.</typeparam>
/// <typeparam name="TResultB">The type of the result produced by the second branch handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="branchANext">The handler to execute for the first branch.</param>
/// <param name="branchBNext">The handler to execute for the second branch.</param>
/// <param name="ct">The cancellation token to observe.</param>
/// <returns>A value task representing the asynchronous operation.</returns>
public delegate ValueTask<TResult> ForkSelector<TInput, TResult, TBranchAInput, TResultA, TBranchBInput, TResultB>(
    TInput input,
    Handler<TBranchAInput, TResultA> branchANext,
    Handler<TBranchBInput, TResultB> branchBNext,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a multi-fork selector that takes an input, a dictionary of branch handlers, and a default handler.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the selector.</typeparam>
/// <typeparam name="TBranchesInput">The type of the input data for the branch handlers.</typeparam>
/// <typeparam name="TBranchesResult">The type of the result produced by the branch handlers.</typeparam>
/// <typeparam name="TDefaultInput">The type of the input data for the default handler.</typeparam>
/// <typeparam name="TDefaultResult">The type of the result produced by the default handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="branches">The dictionary of branch handlers.</param>
/// <param name="defaultNext">The default handler to execute if no branch matches.</param>
/// <param name="ct">The cancellation token to observe.</param>
/// <returns>A value task representing the asynchronous operation.</returns>
public delegate ValueTask<TResult> MultiForkSelector<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>(
    TInput input,
    IReadOnlyDictionary<string, Handler<TBranchesInput, TBranchesResult>> branches,
    Handler<TDefaultInput, TDefaultResult> defaultNext,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a pipe that takes an input and a next handler.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the pipe.</typeparam>
/// <typeparam name="TNextInput">The type of the input data for the next handler.</typeparam>
/// <typeparam name="TNextResult">The type of the result produced by the next handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="next">The handler to execute after the current handler.</param>
/// <param name="ct">The cancellation token to observe.</param>
/// <returns>A value task representing the asynchronous operation.</returns>
public delegate ValueTask<TResult> Pipe<TInput, TResult, out TNextInput, TNextResult>(
    TInput input,
    Handler<TNextInput, TNextResult> next,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a handler that processes an input and returns a value task with a result.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="ct">The cancellation token to observe.</param>
/// <returns>A value task representing the asynchronous operation.</returns>
public delegate ValueTask<TResult> Handler<in TInput, TResult>(
    TInput input,
    CancellationToken ct = default);


