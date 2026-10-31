namespace HospitalAppointment.Client.Services;

public sealed record ApiResult<T>(T? Value, string? Error)
{
    public bool IsSuccess => Error is null;

    public static ApiResult<T> Success(T value) => new(value, null);

    public static ApiResult<T> Failed(string error) => new(default, error);
}
