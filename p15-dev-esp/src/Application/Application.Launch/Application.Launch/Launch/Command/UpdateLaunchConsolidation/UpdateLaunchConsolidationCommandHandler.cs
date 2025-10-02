using Domain.Models;
using FluentValidation;
using MediatR;
using Services.Interfaces;

namespace Application.Launch.Launch.Command.UpdateLaunchConsolidation;
public class UpdateLaunchConsolidationCommandHandler(ILaunchService _service, IValidator<UpdateLaunchConsolidationCommand> _validator) : IRequestHandler<UpdateLaunchConsolidationCommand, ApiResponse>
{

    public async Task<ApiResponse> Handle(UpdateLaunchConsolidationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ApiResponse { Success = false, Message = string.Join(";", validationResult.Errors.Select(e => e.ErrorMessage).ToList()) };
        }

        return await _service.UpdateLaunchConsolidation(
           request.Launches
        );
    }
}