using Domain.Models;
using FluentValidation;
using MediatR;
using Services.Interfaces;

namespace Application.Authentication.Authentication.Command.Register;
public class RegisterCommandHandler(IAuthService _service, IValidator<RegisterCommand> _validator) : IRequestHandler<RegisterCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ApiResponse { Success = false, Message = string.Join(";", validationResult.Errors.Select(e => e.ErrorMessage).ToList()) };
        }

        return await _service.Register(request.UserName, request.FullName, request.Password);
    }
}