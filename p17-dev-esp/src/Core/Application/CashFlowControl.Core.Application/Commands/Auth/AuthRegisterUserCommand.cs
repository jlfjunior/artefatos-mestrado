using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using Flunt.Notifications;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CashFlowControl.Core.Application.Commands
{
    public class AuthRegisterUserCommand : Notifiable<Notification>, IRequest<Result<(IdentityResult? identityResult, ApplicationUser applicationUser)>>
    {
        public RegisterModelDTO RegisterModelDTO;

        public AuthRegisterUserCommand(RegisterModelDTO registerModelDTO)
        {
            RegisterModelDTO = registerModelDTO;
        }
    }
}
