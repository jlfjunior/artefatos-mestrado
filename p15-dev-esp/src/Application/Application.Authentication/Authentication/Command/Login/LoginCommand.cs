using Domain.Models.Authentication.Login;
using MediatR;

namespace Application.Authentication.Authentication.Command.Login;
public class LoginCommand : IRequest<LoginResponseModel>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}