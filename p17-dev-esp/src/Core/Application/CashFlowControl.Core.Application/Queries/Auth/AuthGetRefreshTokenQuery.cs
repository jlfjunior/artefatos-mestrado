using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using Flunt.Notifications;
using MediatR;

namespace CashFlowControl.Core.Application.Queries.Auth
{
    public class AuthGetRefreshTokenQuery : Notifiable<Notification>, IRequest<Result<RefreshToken?>>
    {
        public string RefreshToken;

        public AuthGetRefreshTokenQuery(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
