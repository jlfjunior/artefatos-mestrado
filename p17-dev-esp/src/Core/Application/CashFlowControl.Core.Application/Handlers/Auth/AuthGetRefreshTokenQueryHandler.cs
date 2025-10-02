using CashFlowControl.Core.Application.Exceptions;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Queries.Auth;
using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using MediatR;

namespace CashFlowControl.Core.Application.Handlers
{
    public class AuthGetRefreshTokenQueryHandler : IRequestHandler<AuthGetRefreshTokenQuery, Result<RefreshToken?>>
    {
        private readonly IUserRepository _userRepository;

        public AuthGetRefreshTokenQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<RefreshToken?>> Handle(AuthGetRefreshTokenQuery request, CancellationToken cancellationToken)
        {
            if (!request.IsValid)
            {
                throw new CommandValidationException(request.Notifications);
            }

            try
            {
                return await Task.FromResult(Result<RefreshToken?>.Success(_userRepository.GetRefreshToken(request.RefreshToken)));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(Result<RefreshToken?>.ValidationFailure(ex.Message));
            }
        }

    }
}
