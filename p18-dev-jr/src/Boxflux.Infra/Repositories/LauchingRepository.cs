using Boxflux.Domain.Interfaces;
using Boxflux.Infra.Context;
using Microsoft.EntityFrameworkCore;

public class LauchingRepository : IGeralRepository<Lauching>
{
    private readonly BoxfluxContext _context;

    public LauchingRepository(BoxfluxContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(Guid id, Lauching item)
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

    public async  Task CreateAsync(Lauching item)
    {
       await _context.Lauchings.AddAsync(item);
       await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Lauching item)
    {
        var lauch = await _context.Lauchings.FindAsync(item.Id);
        _context.Lauchings.Remove(lauch);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Lauching>> GetAllAsync()
    {
        return await _context.Lauchings.ToListAsync();
    }

    public async Task<IEnumerable<Lauching>> GetAllByDateAsync(DateTime date)
    {
       return await _context.Lauchings.Where(item => item.DateLauching.Date == date).ToListAsync();
    }

    public async Task<Lauching> GetByDateAsync(DateTime date)
    {
        return await _context.Lauchings.FirstOrDefaultAsync(item => item.DateLauching == date);
    }

    public Task<Lauching> GetByIdAsync(Guid id)
    {
        return _context.Lauchings.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(Lauching item)
    {
        _context.Lauchings.Update(item);
        await _context.SaveChangesAsync();
    }
}