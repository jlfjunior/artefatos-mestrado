using EasyCash.Abstractions.Authentication;
using EasyCash.Abstractions.Interfaces;
using EasyCash.Domain.Users.Entities;
using EasyCash.Domain.Users.Interfaces;

namespace EasyCash.Api.Extensions;

internal static class SeedDataExtensions
{
    public static void SeedData(this IApplicationBuilder app, IHostEnvironment env)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        if (env.IsDevelopment())
            TryCreateTestUser(scope);
    }

    private static void TryCreateTestUser(IServiceScope scope)
    {
        try
        {
            var registerService = scope.ServiceProvider.GetRequiredService<IRegisterService<UserEntity>>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var user = UserEntity.InitialTestUser;

            var result = registerService.RegisterAsync(user, "123456", CancellationToken.None).GetAwaiter().GetResult();

            user.SetIdentityId(result);

            var transaction = unitOfWork.BeginTransactionAsync(CancellationToken.None).GetAwaiter().GetResult();

            userRepository.AddAsync(user, CancellationToken.None).GetAwaiter().GetResult();

            unitOfWork.SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();

            transaction.Commit();
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}
