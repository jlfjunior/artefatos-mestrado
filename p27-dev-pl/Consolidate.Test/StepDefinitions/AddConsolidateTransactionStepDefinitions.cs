using Consolidate.Business.Model;
using System;
using TechTalk.SpecFlow;
using Foundation.Utils;
using Foundation.Business.DomainNotitications;
using MediatR;
using Consolidate.Business.UseCase.AddConsolidateTransaction;

namespace Consolidate.Test.StepDefinitions
{
    [Binding]
    public class AddConsolidateTransactionStepDefinitions
    {
        AddConsolidateTransactionInputDto input;
        private readonly IMediator _mediator;
        private readonly DomainNotificationContext _domainNotificaionContext;

        public AddConsolidateTransactionStepDefinitions(IMediator mediator, DomainNotificationContext domainNotificaionContext)
        {
            _mediator = mediator;
            _domainNotificaionContext = domainNotificaionContext;
        }

        [Given(@"a ""([^""]*)"" and value ""([^""]*)"" and date ""([^""]*)"" and type of ""([^""]*)""")]
        public void GivenAAndValueAndDateAndTypeOf(string description, string p1, string p2, string credit)
        {
            input = new AddConsolidateTransactionInputDto(Guid.NewGuid(), description, Double.Parse(p1), DateTime.Parse(p2), credit.ToEnum<TransactionType>());
        }

        [When(@"AddConsolidateTransaction")]
        public async void WhenAddConsolidateTransaction()
        {
            await _mediator.Send(input);    
        }

        [Then(@"message should be ""([^""]*)""")]
        public void ThenMessageShouldBe(string p0)
        {
            if (p0 == string.Empty)
            {
                _domainNotificaionContext.IsValid.Should().BeTrue();
                _domainNotificaionContext.Notifications.Any().Should().BeFalse();
                return;
            }

            _domainNotificaionContext.IsValid.Should().BeFalse();            
        }
    }


    
}
