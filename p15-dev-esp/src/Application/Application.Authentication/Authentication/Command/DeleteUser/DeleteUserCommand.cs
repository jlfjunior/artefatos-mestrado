using Domain.Models;
using MediatR;

namespace Application.Authentication.Authentication.Command.DeleteUser;

public class DeleteUserCommand : IRequest<ApiResponse>
{
    public string Id { get; set; }
}