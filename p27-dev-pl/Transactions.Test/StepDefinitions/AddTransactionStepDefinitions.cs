using MediatR;
using Transactions.Business.Model;
using Transactions.Business.UseCase;
using Foundation.Business.DomainNotitications;

namespace Transactions.Test.StepDefinitions
{
    [Binding]
    public class AddTransactionStepDefinitions
    {
        private TransactionInputDto input;
        private readonly IMediator mediator;
        private readonly DomainNotificationContext domainNotificaionContext;

        public AddTransactionStepDefinitions(IMediator mediator, DomainNotificationContext domainNotificaionContext)
        {
            this.mediator = mediator;
            this.domainNotificaionContext = domainNotificaionContext;
        }

        [Given(@"a value (.*)")]
        public void GivenAValue(int p0)
        {
            input = new("", p0, TransactionType.Credit);
        }

        [When(@"add transaction")]
        public async void WhenAddTransaction()
        {
            await mediator.Send(input);
        }

        [Then(@"message should be ""([^""]*)""")]
        public void ThenMessageShouldBe(string p0)
        {
            if(p0 == string.Empty)
            {
                domainNotificaionContext.IsValid.Should().BeTrue();
                domainNotificaionContext.Notifications.Any().Should().BeFalse();
                return;
            }

            domainNotificaionContext.IsValid.Should().BeFalse();
            domainNotificaionContext.Notifications.Contains(Transaction.VALUE_MUST_HAVE_GRATHER_THAN_0);
        }

        [Given(@"a ""([^""]*)"" and value (.*) and type (.*)")]
        public void GivenAAndValueAndType(string description, int p1, int p2)
        {
            input = new(description, p1, (TransactionType)p2);
        }


    }


}
