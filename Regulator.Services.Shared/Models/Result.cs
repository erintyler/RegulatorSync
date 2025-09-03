using System.Diagnostics.CodeAnalysis;

namespace Regulator.Services.Shared.Models;

public class Result<T>(bool isSuccess, T? value, string? errorMessage, int statusCode) where T : class
{
    public static Result<T> Success(T value) => new(true, value, null, 200);
    public static Result<T> Failure(string errorMessage, int statusCode = 500) => new(false, null, errorMessage, statusCode);
    
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess { get; } = isSuccess;
    public T? Value { get; } = value;
    public string? ErrorMessage { get; } = errorMessage;
    public int StatusCode { get; } = statusCode;
}

public class Result(bool isSuccess, string? errorMessage, int statusCode)
{
    public static Result Success() => new(true, null, 200);
    public static Result Failure(string errorMessage, int statusCode = 500) => new(false, errorMessage, statusCode);
    
    [MemberNotNullWhen(true, nameof(IsSuccess))]
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess { get; init; } = isSuccess;
    public string? ErrorMessage { get; init; } = errorMessage;
    public int StatusCode { get; init; } = statusCode;
}