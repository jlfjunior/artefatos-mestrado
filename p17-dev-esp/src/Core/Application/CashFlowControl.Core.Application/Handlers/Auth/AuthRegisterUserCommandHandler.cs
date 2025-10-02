using CashFlowControl.Core.Application.Commands;
using CashFlowControl.Core.Application.Exceptions;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CashFlowControl.Core.Application
{
    public class AuthRegisterUserCommandHandler : IRequestHandler<AuthRegisterUserCommand, Result<(IdentityResult? identityResult, ApplicationUser applicationUser)>>
    {
        private readonly IUserRepository _userRepository;

        public AuthRegisterUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<(IdentityResult? identityResult, ApplicationUser applicationUser)>> Handle(AuthRegisterUserCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsValid)
            {
                throw new CommandValidationException(request.Notifications);
            }

            try
            {
                return Result<(IdentityResult? identityResult, ApplicationUser applicationUser)>.Success(await _userRepository.RegisterUser(request.RegisterModelDTO));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(Result<(IdentityResult? identityResult, ApplicationUser applicationUser)>.ValidationFailure(ex.Message));
            }
        }

    }
}
