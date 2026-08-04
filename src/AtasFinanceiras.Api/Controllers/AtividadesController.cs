using AtasFinanceiras.Application.DTOs.Atividades;
using AtasFinanceiras.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtasFinanceiras.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AtividadesController : ControllerBase
{
    private readonly IAtividadeService _service;
    private readonly ICurrentUserService _currentUser;

    public AtividadesController(IAtividadeService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // Público de propósito: o site institucional (sem login) também loga ações
    // como "download de documento" — nesse caso o registro fica com usuário nulo,
    // igual ao comportamento original (é o front quem decide o que logar).
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Log(LogAtividadeRequest request, CancellationToken ct)
    {
        await _service.RegistrarAsync(_currentUser.UserId, request.Acao, request.Documento, ct);
        return NoContent();
    }

    // Feed de atividades e notificações só existe dentro do painel administrativo.
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRecentes([FromQuery] int limit, CancellationToken ct)
    {
        var atividades = await _service.GetRecentesAsync(_currentUser.UserId, limit == 0 ? 6 : limit, ct);
        return Ok(atividades);
    }

    [HttpPost("{id:guid}/visualizada")]
    [Authorize]
    public async Task<IActionResult> ToggleVisualizada(Guid id, [FromQuery] bool viewed, CancellationToken ct)
    {
        await _service.ToggleNotificacaoVisualizadaAsync(_currentUser.UserId!.Value, id, viewed, ct);
        return NoContent();
    }
}
