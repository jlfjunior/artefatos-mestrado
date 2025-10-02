using Domain.Models;
using MediatR;

namespace Application.Authentication.Authentication.Command.ChangePassword;
public class ChangePasswordCommand : IRequest<ApiResponse>
{
    public string Username { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}