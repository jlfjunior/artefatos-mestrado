using CashFlow.BuildingBlocks.Results;

namespace CashFlow.Launches.Api.Extensions;

public static class ErrorHttpMapper
{
    public static IResult ToHttpResult(this Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => Results.ValidationProblem(
                error.Details ?? new Dictionary<string, string[]>
                {
                    { "Validation", [error.Message] }
                }),
            ErrorType.Business => Results.BadRequest(new { error.Code, error.Message }),
            ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
            ErrorType.Conflict => Results.Conflict(new { error.Code, error.Message }),

            _ => Results.Problem(
                title: "Unexpected error",
                detail: error.Message,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}