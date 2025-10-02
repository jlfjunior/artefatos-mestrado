using AutoMapper;
using Infrastructure.Configurations;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailySummary.Tests;

public abstract class BaseTests
{
    protected DailySummaryDbContext DbContext = null!;
    protected IMapper Mapper = null!;

    [SetUp]
    public void BaseSetUp()
    {
        var dbOptions = new DbContextOptionsBuilder<DailySummaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new DailySummaryDbContext(dbOptions);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DailySummaryProfile>();
        });

        Mapper = mapperConfig.CreateMapper();
    }

    [TearDown]
    public void TearDown()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}