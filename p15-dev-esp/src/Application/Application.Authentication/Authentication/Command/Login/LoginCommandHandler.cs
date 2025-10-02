using Domain.Models.Authentication.Login;
using FluentValidation;
using MediatR;
using Services.Interfaces;

namespace Application.Authentication.Authentication.Command.Login;
public class LoginCommandHandler(IAuthService _service, IValidator<LoginCommand> _validator) : IRequestHandler<LoginCommand, LoginResponseModel>
{
    public async Task<LoginResponseModel> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new LoginResponseModel { Success = false, Message = string.Join(";", validationResult.Errors.Select(e => e.ErrorMessage).ToList()) };
        }

        return await _service.Authenticate(request.Username, request.Password);
    }
}