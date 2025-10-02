using Domain.Models;
using FluentValidation;
using MediatR;
using Services.Interfaces;

namespace Application.Launch.Launch.Command.CreateLaunch;
public class CreateLaunchCommandHandler(ILaunchService _service, IValidator<CreateLaunchCommand> _validator) : IRequestHandler<CreateLaunchCommand, ApiResponse>
{

    public async Task<ApiResponse> Handle(CreateLaunchCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ApiResponse { Success = false, Message = string.Join(";", validationResult.Errors.Select(e => e.ErrorMessage).ToList()) };
        }

        return await _service.CreateLaunch(
           request.LaunchType,
           request.ProductsOrder
        );
    }
}