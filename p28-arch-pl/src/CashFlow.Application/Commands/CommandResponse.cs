namespace CashFlow.Application.Commands;

public record CommandResponse<T>
{
    private CommandResponse()
    {
    }

    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public Error? Error { get; private set; }

    public void Deconstruct(out bool isSuccess, out T? data, out Error? error)
    {
        isSuccess = IsSuccess;
        data = Data;
        error = Error;
    }

    public static CommandResponse<T> CreateSuccess(T data)
    {
        return new CommandResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Error = null
        };
    }

    public static CommandResponse<T> CreateFail(ErrorCode errorCode, string message)
    {
        return new CommandResponse<T>
        {
            IsSuccess = false,
            Data = default,
            Error = new Error(errorCode, message)
        };
    }
}

public record Error(ErrorCode ErrorCode, string Message);