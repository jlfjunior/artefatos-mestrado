using AutoMapper;
using Infrastructure.Configurations;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Transactions.Tests;

public abstract class BaseTests
{
    protected TransactionsDbContext DbContext = null!;
    protected Mock<IPublishEndpoint> PublishEndpointMock = null!;
    protected IMapper Mapper = null!;

    [SetUp]
    public void BaseSetUp()
    {
        var dbOptions = new DbContextOptionsBuilder<TransactionsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new TransactionsDbContext(dbOptions);
        PublishEndpointMock = new Mock<IPublishEndpoint>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TransactionProfile>();
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