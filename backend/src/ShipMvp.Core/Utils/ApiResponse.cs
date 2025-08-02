namespace ShipMvp.Core;

// API Response wrapper
public class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public string? Message { get; init; }

    public static ApiResponse<T> Success(T data, string? message = null) => new()
    {
        IsSuccess = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> MarkError(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}
