using Domain.Models;
using MediatR;
using Services.Interfaces;

namespace Application.Authentication.Authentication.Command.DeleteUser;
public class DeleteUserCommandHandler(IAuthService _service) : IRequestHandler<DeleteUserCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken) => await _service.DeleteUser(request.Id);
}