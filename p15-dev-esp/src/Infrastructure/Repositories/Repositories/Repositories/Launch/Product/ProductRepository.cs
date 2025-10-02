using Domain.Entities.Launch;
using Infrastructure.Persistence.Data;
using Repositories.Repositories.Generic.GenericSQL;

namespace Domain.Entities.Product;

public class ProductRepository : Repository<ProductEntity>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }
}