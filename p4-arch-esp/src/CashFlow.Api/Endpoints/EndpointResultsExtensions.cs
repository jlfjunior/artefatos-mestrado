using ErrorOr;

namespace CashFlow.Api.Endpoints
{
    public static class EndpointResultsExtensions
    {
        public static IResult ToProblem(this List<Error> errors)
        {
            if (errors.Count == 0)
            {
                return Results.Problem();
            }

            return CreateProblem(errors);
        }

        private static IResult CreateProblem(List<Error> errors)
        {
            var statusCode = errors.First().Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            return Results.ValidationProblem(
                errors.ToDictionary(k => k.Code, v => new[] { v.Description }),
                statusCode: statusCode);
        }
    }

}
