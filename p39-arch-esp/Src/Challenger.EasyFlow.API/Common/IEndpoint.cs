namespace Challenger.EasyFlow.API.Common;

public interface IEndpoint
{
    abstract static void Map(IEndpointRouteBuilder builder);
}
