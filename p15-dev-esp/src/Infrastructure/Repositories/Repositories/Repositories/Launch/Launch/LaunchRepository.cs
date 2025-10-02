using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories.Generic.GenericSQL;

namespace Domain.Entities.Launch;

public class LaunchRepository : Repository<LaunchEntity>, ILaunchRepository
{
    public LaunchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IList<LaunchEntity>> GetAllAsync()
    {
        return await _dbSet.Include(i => i.LaunchProducts).ToListAsync();
    }
}