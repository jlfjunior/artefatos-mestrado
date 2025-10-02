namespace Domain.Models.Authentication.Login
{
    public class LoginResponseModel : ApiResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
