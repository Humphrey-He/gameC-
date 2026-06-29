namespace GamePlayerSystem.Api.Responses;

public sealed class ErrorResponse
{
    public string Error { get; init; } = string.Empty;

    public static ErrorResponse From(string error)
    {
        return new ErrorResponse
        {
            Error = error
        };
    }
}
