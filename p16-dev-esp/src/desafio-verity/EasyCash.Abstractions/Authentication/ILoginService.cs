namespace EasyCash.Abstractions.Authentication;

public interface ILoginService
{
    Task<string> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
