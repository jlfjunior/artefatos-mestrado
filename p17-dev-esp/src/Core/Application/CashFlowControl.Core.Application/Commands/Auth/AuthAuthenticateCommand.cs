using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using Flunt.Notifications;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CashFlowControl.Core.Application.Commands
{
    public class AuthAuthenticateCommand : Notifiable<Notification>, IRequest<Result<(SignInResult? identityResult, ApplicationUser? applicationUser)>>
    {
        public LoginModelDTO LoginModelDTO;

        public AuthAuthenticateCommand(LoginModelDTO loginModelDTO)
        {
            LoginModelDTO = loginModelDTO;
        }
    }
}
