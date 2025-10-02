using FluxoCaixa.Application.Commands;
using FluxoCaixa.Application.Queries;
using FluxoCaixa.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FluxoCaixa.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LancamentosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LancamentosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarLancamentoCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lancamentos = await _mediator.Send(new ObterTodosLancamentosQuery());
            return Ok(lancamentos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lancamento = await _mediator.Send(new ObterLancamentoPorIdQuery(id));
            return lancamento != null ? Ok(lancamento) : NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] AtualizarLancamentoCommand command)
        {
            if (id != command.Id)
                return BadRequest();

            var atualizado = await _mediator.Send(command);
            return atualizado ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var excluido = await _mediator.Send(new ExcluirLancamentoCommand(id));
            return excluido ? NoContent() : NotFound();
        }
    }
}
