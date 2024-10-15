using System;
using System.Diagnostics.CodeAnalysis;

namespace SimulationServer.Business.Infrastructure;

public class Result<T>
{
    // Success constructor
    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
    }

    // Failure constructor
    private Result(Exception error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Exception? Error { get; }

    // Helper methods for constructing the `Result<T>`
    public static Result<T> Success(T value) => new(value);
    public static Result<T> Fail(Exception error) => new(error);

    // Allow converting a T directly into Result<T>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }
}