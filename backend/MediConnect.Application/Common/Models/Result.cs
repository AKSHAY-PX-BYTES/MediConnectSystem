namespace MediConnect.Application.Common.Models;

/// <summary>A simple result wrapper used by command/query handlers.</summary>
public class Result
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(string error) => new() { Succeeded = false, Error = error };
}

public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Success(T data) => new() { Succeeded = true, Data = data };
    public static new Result<T> Failure(string error) => new() { Succeeded = false, Error = error };
}

/// <summary>Standard paged response.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
