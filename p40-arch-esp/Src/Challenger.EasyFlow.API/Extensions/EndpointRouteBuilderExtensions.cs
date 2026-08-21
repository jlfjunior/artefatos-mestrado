using Challenger.EasyFlow.API.Common;

namespace Challenger.EasyFlow.API.Extensions;

public static class EndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        public void MapEndpoint<TEndpoint>() where TEndpoint : IEndpoint
        {
            TEndpoint.Map(builder);
        }
    }
}