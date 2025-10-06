using Foundation.Business.DomainNotitications;
using Microsoft.Extensions.Logging;

namespace Transactions.Test
{
    public class TestDomainNotificationContext : DomainNotificationContext
    {
        public TestDomainNotificationContext(ILogger<TestDomainNotificationContext> logger) : base(logger)
        {
        }
    }
}
