using Foundation.Business.DomainNotitications;
using Microsoft.Extensions.Logging;

namespace Consolidate.Test
{
    public class TestDomainNotificationContext : DomainNotificationContext
    {
        public TestDomainNotificationContext(ILogger<TestDomainNotificationContext> logger) : base(logger)
        {
        }
    }
}