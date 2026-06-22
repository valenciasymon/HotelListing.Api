
namespace HotelListing.Api.Results;

public readonly record struct Result
{
    public bool IsSuccess { get; }

    public Error[] Errors { get; }

    private Result(bool isSuccess, Error[] errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success()
    {
        return new Result(true,[]);
    }

    public static Result Failure(params Error[] errors)
    {
        return new Result(false,errors);
    }
    public static Result NotFound(params Error[] errors)
    {
        return new Result(false,errors);
    }
    public static Result BadRequest(params Error[] errors)
    {
        return new Result(false, errors);
    }
    public static Result Conflict(params Error[] errors)
    {
        return new Result(false, errors);
    }

    public static Result Combine(params Result[] results)
    {
        return results.Any(r => !r.IsSuccess) ?
            Failure(results.Where(r => !r.IsSuccess).SelectMany(r => r.Errors).ToArray())
            :Success();
    }
}


public readonly record struct Result<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    public Error[] Errors { get; }

    private Result(bool isSuccess, T? value, Error[] errors)
        => (IsSuccess, Value, Errors) = (isSuccess, value, errors);

    public static Result<T> Success(T value)
        => new(true, value, []);

    public static Result<T> Failure(params Error[] errors)
        => new(false, default, errors);

    public static Result<T> NotFound(params Error[] errors)
        => new(false, default, errors);

    public static Result<T> BadRequest(params Error[] errors)
        => new(false, default, errors);

    public static Result<T> Conflict(params Error[] errors)
        => new(false, default, errors);
}
