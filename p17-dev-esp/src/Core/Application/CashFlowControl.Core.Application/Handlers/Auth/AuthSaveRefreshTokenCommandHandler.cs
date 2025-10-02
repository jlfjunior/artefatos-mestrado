using CashFlowControl.Core.Application.Commands.Auth;
using CashFlowControl.Core.Application.Exceptions;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Security.Helpers;
using MediatR;

namespace CashFlowControl.Core.Application.Handlers
{
    public class AuthSaveRefreshTokenCommandHandler : IRequestHandler<AuthSaveRefreshTokenCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository;

        public AuthSaveRefreshTokenCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<bool>> Handle(AuthSaveRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsValid)
            {
                throw new CommandValidationException(request.Notifications);
            }

            try
            {
                _userRepository.SaveRefreshToken(request.RefreshToken);
                return await Task.FromResult(Result<bool>.Success(true));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(Result<bool>.ValidationFailure(ex.Message));
            }            
        }
    }
}
