using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Domain.Entities.Consolidation;
using Domain.Models.Launch.Launch;
using Domain.DTOs.Consolidation;
using Domain.DTOs.Launch;
using Domain.DTOs;

namespace Application.Authentication.Services;

public class ConsolidationService(
        IConsolidationRepository _repository,
        ILogger<ConsolidationService> _logger) : IConsolidationService
{

    public async Task<List<LaunchDTO>> CreateConsolidation(List<LaunchDTO> launches)
    {
        try
        {
            decimal totalAmount = 0;
            decimal totalCreditAmount = 0;
            decimal totalDebitAmount = 0;

            foreach (var launch in launches)
            {
                totalAmount += launch.Amount;

                if (launch.LaunchType == LaunchTypeEnum.Credit)
                    totalCreditAmount += launch.Amount;
                else
                    totalDebitAmount += launch.Amount;
            }

            await _repository.InsertAsync(new ConsolidationEntity
            {
                Date = DateTime.UtcNow,
                TotalAmount = totalAmount,
                TotalCreditAmount = totalCreditAmount,
                TotalDebitAmount = totalDebitAmount
            });

            launches.ForEach(l =>
            {
                l.Status = ConsolidationStatusEnum.Consolidated;
                l.ModificationDate = DateTime.UtcNow;
            });

            return launches;
        }
        catch (Exception ex)
        {
            launches.ForEach(l =>
            {
                l.Status = ConsolidationStatusEnum.Error;
                l.ModificationDate = DateTime.UtcNow;
            });

            _logger.LogError(ex, "An error occurred while creating a Consolidation.");
            return launches;
        }
    }

    public async Task<List<ConsolidationDTO>> GetAll()
    {
        var consolidations = await _repository.GetAllAsync();

        return consolidations.Select(c => new ConsolidationDTO
        {
            Id = c.Id,
            Date = c.Date,
            TotalAmount = c.TotalAmount,
            TotalCreditAmount = c.TotalCreditAmount,
            TotalDebitAmount = c.TotalDebitAmount
        }).ToList();
    }

    public async Task<PaginatedResult<ConsolidationDTO>> GetPaginated(int pageNumber, int pageSize)
    {
        var consolidationsAll = await _repository.GetAllAsync();

        var consolidationsPaginated = consolidationsAll.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var paginatedConsolidations = consolidationsPaginated.Select(p => new ConsolidationDTO
        {
            Id = p.Id,
            Date = p.Date,
            TotalAmount = p.TotalAmount,
            TotalCreditAmount = p.TotalCreditAmount,
            TotalDebitAmount = p.TotalDebitAmount
        }).ToList();

        return new PaginatedResult<ConsolidationDTO>(paginatedConsolidations, consolidationsAll.Count, pageSize, pageNumber);
    }
}