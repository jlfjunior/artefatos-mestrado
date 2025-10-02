using CashFlowControl.Core.Application.Commands;
using CashFlowControl.Core.Application.Exceptions;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CashFlowControl.Core.Application.Handlers
{
    public class AuthAuthenticateCommandHandler : IRequestHandler<AuthAuthenticateCommand, Result<(SignInResult? identityResult, ApplicationUser? applicationUser)>>
    {
        private readonly IUserRepository _userRepository;

        public AuthAuthenticateCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<(SignInResult? identityResult, ApplicationUser? applicationUser)>> Handle(AuthAuthenticateCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsValid)
            {
                throw new CommandValidationException(request.Notifications);
            }

            try
            {
                return Result<(SignInResult? identityResult, ApplicationUser? applicationUser)>.Success(await _userRepository.Authenticate(request.LoginModelDTO));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(Result<(SignInResult? identityResult, ApplicationUser? applicationUser)>.ValidationFailure(ex.Message));
            }
        }
    }
}
