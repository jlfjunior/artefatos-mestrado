using DnsClient.Internal;
using Domain.DTOs.Launch;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Application.Consolidation.Consolidation.Command.CreateConsolidation;
public class CreateConsolidationCommandHandler(IConsolidationService _service, IValidator<CreateConsolidationCommand> _validator, ILogger<CreateConsolidationCommandHandler> _logger) : IRequestHandler<CreateConsolidationCommand, List<LaunchDTO>>
{

    public async Task<List<LaunchDTO>> Handle(CreateConsolidationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogError("Error on creating consolidation: {0}", string.Join(";", validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
            request.Launches.ForEach(l =>
            {
                l.Status = Domain.Models.Launch.Launch.ConsolidationStatusEnum.Error;
                l.ModificationDate = DateTime.UtcNow;
            });

            return request.Launches;
        }

        return await _service.CreateConsolidation(request.Launches);
    }
}