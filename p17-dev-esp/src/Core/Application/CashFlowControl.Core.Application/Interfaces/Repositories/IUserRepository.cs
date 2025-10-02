using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CashFlowControl.Core.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<(IdentityResult? identityResult, ApplicationUser applicationUser)> RegisterUser(RegisterModelDTO model);
        Task<(SignInResult? result, ApplicationUser? user)> Authenticate(LoginModelDTO model);
        void SaveRefreshToken(RefreshToken refreshToken);
        RefreshToken? GetRefreshToken(string token);
    }
}
