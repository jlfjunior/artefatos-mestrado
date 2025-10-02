//using Application.Transaction.Handlers;
//using Moq;
//using Shared.Enums;
//using Shared.Messages;

//namespace Transactions.Tests.Handlers;

//[TestFixture]
//public class CreateTransactionHandlerTests : BaseTests
//{
//    private CreateTransaction _handler = null!;

//    [SetUp]
//    public void SetUp()
//    {
//        _handler = new CreateTransaction(DbContext, PublishEndpointMock.Object);
//    }

//    [Test]
//    public async Task Handle_ValidTransaction_ShouldCreateTransactionAndPublishEvent()
//    {
//        // Arrange
//        var command = new CreateTransaction.CreateTransactionCommand(100.00m, TransactionType.Credit);

//        // Act
//        var transactionId = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.That(transactionId, Is.Not.EqualTo(Guid.Empty));

//        PublishEndpointMock.Verify(x =>
//            x.Publish(It.IsAny<TransactionCreated>(), It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Test]
//    public void Handle_InvalidAmount_ShouldThrowException()
//    {
//        // Arrange
//        var command = new CreateTransaction.CreateTransactionCommand(-10.00m, TransactionType.Credit);

//        // Act
//        async Task act()
//        {
//            await _handler.Handle(command, CancellationToken.None);
//        }

//        // Assert
//        Assert.That(async () => await act(), Throws.Exception.With.Message.EqualTo("O valor deve ser maior que zero."));

//        PublishEndpointMock.Verify(x =>
//            x.Publish(It.IsAny<TransactionCreated>(), It.IsAny<CancellationToken>()), Times.Never);
//    }
//}
