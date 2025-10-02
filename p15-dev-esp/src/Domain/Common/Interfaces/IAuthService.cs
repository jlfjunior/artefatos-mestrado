namespace Common.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseModel> Authenticate(string username, string password);

    }
}
