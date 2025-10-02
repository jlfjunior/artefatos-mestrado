using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using Flunt.Notifications;
using MediatR;

namespace CashFlowControl.Core.Application.Commands.Auth
{
    public class AuthSaveRefreshTokenCommand : Notifiable<Notification>, IRequest<Result<bool>>
    {
        public RefreshToken RefreshToken;

        public AuthSaveRefreshTokenCommand(RefreshToken refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
