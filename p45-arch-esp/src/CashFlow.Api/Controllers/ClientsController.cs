using CashFlow.Application.Authorization;
using CashFlow.Contracts;
using CashFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.Controllers;

/// <summary>
/// Consulta de usuários clientes cadastrados.
/// </summary>
/// <param name="service">Serviço de consulta de clientes.</param>
[ApiController]
[Route("api/clients")]
[Tags("Clientes")]
[Authorize]
[Produces("application/json")]
public sealed class ClientsController(ClientService service) : ControllerBase
{
    /// <summary>
    /// Lista usuários com perfil cliente.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de clientes.</returns>
    /// <response code="200">Clientes cadastrados.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Referência de acesso insuficiente.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.UsersRead)]
    [ProducesResponseType(typeof(IReadOnlyList<ClientResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientResponse>>> List(CancellationToken cancellationToken = default)
    {
        var items = await service.ListClientsAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retorna o perfil do cliente autenticado.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Perfil do cliente autenticado ou null se não encontrado.</returns>
    /// <response code="200">Perfil do cliente autenticado.</response>
    /// <response code="404">Cliente não encontrado.</response>
    [HttpGet("me")]
    [Authorize(Policy = AuthorizationPolicies.UsersReadSelf)]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetMe(CancellationToken cancellationToken = default)
    {
        var client = await service.GetCurrentClientAsync(cancellationToken);
        return client is null ? NotFound() : Ok(client);
    }

    /// <summary>
    /// Obtém um cliente pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do cliente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Cliente ou null se não encontrado.</returns>
    /// <response code="200">Cliente encontrado.</response>
    /// <response code="403">Cliente autenticado tentando acessar outro perfil.</response>
    /// <response code="404">Cliente não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ClientProfileRead)]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await service.GetClientAsync(id, cancellationToken);
        return client is null ? NotFound() : Ok(client);
    }
}
