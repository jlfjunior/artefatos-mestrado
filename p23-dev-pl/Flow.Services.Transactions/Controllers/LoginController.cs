using Flow.Application.UseCases.Login;
using Flow.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Services.Transactions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> DoLogin([FromBody] DoLoginRequest request, [FromServices] IDoLoginUseCase useCase)
        {
            var token = await useCase.Execute(request);
            return Ok(new DoLoginResponse()
            {
                Token = token
            });
        }
    }
}
