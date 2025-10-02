using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Domain.Entities;
using CashFlowControl.Core.Infrastructure.Contexts;
using Microsoft.AspNetCore.Identity;

namespace CashFlowControl.Core.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(IdentityResult? identityResult, ApplicationUser applicationUser)> RegisterUser(RegisterModelDTO model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            return (result, user);
        }

        public async Task<(SignInResult? result, ApplicationUser? user)> Authenticate(LoginModelDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return (null, null);

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            return (result, user);
        }

        public void SaveRefreshToken(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            _context.SaveChanges();
        }

        public RefreshToken? GetRefreshToken(string token)
        {
            return _context.RefreshTokens.FirstOrDefault(rt => rt.Token == token);
        }
    }
}
