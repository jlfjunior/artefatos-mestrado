using Cashflow.SharedKernel.Token;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Operations.Api.Controllers
{
    [ApiController]
    [Route("api/")]
    public class TokenController : Controller
    {
        [HttpGet("token/generate")]
        public IActionResult GenerateToken()
        {
            var token = new TokenGenerator();
            return Ok(token.Generate());
        }
    }
}
