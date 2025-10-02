using Boxflux.Domain.Interfaces;
using Boxflux.Infra.Context;
using Microsoft.EntityFrameworkCore;

public class ConsolidatedDailyRepository : IGeralRepository<ConsolidatedDaily>
{
    private readonly BoxfluxContext _context;
    public ConsolidatedDailyRepository(BoxfluxContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(Guid id, ConsolidatedDaily item)
    {
        var existingItem = await GetByIdAsync(id);
        if (existingItem != null)
        {
            await UpdateAsync(item);
        }
        else
        {
            await CreateAsync(item);
        }
    }

    public async Task CreateAsync(ConsolidatedDaily item)
    {
        await _context.ConslidatedDaylies.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ConsolidatedDaily item)
    {
        var dayli = await _context.ConslidatedDaylies.FindAsync(item.Id);
        _context.ConslidatedDaylies.Remove(dayli);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ConsolidatedDaily>> GetAllAsync()
    {
        return await _context.ConslidatedDaylies.ToListAsync();
    }

    public async Task<IEnumerable<ConsolidatedDaily>> GetAllByDateAsync(DateTime date)
    {
        return await _context.ConslidatedDaylies
                              .Where(item => item.DateConsolidate.Date == date.Date)
                              .ToListAsync();
    }

    public async Task<ConsolidatedDaily> GetByDateAsync(DateTime date)
    {
        return await _context.ConslidatedDaylies.
            FirstOrDefaultAsync(item => item.DateConsolidate == date);
    }

    public async Task<ConsolidatedDaily> GetByIdAsync(Guid id)
    {
        return await _context.ConslidatedDaylies.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task UpdateAsync(ConsolidatedDaily item)
    {
        _context.ConslidatedDaylies.Update(item);
        await _context.SaveChangesAsync();
    }
}