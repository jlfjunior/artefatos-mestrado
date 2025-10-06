using Flow.Services.Transactions.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Services.Transactions.Attributes
{
    public class UserAuthenticatedAttribute : TypeFilterAttribute
    {
        public UserAuthenticatedAttribute() : base(typeof(AuthenticatedUserFilter)) { }
    }
}
