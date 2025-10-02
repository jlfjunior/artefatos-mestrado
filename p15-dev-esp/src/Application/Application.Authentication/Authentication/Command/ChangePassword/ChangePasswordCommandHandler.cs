using Domain.Models;
using FluentValidation;
using MediatR;
using Services.Interfaces;

namespace Application.Authentication.Authentication.Command.ChangePassword;
public class ChangePasswordCommandHandler(IAuthService _service, IValidator<ChangePasswordCommand> _validator) : IRequestHandler<ChangePasswordCommand, ApiResponse>
{

    public async Task<ApiResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ApiResponse { Success = false, Message = string.Join(";", validationResult.Errors.Select(e => e.ErrorMessage).ToList()) };
        }

        return await _service.ChangePassword(
            request.Username,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);
    }
}