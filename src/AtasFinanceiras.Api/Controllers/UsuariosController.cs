using AtasFinanceiras.Application.DTOs.Usuarios;
using AtasFinanceiras.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtasFinanceiras.Api.Controllers;

// Gerenciar usuários é uma ação exclusiva do perfil Admin (ver matriz de permissões do painel).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetAll(CancellationToken ct)
    {
        var usuarios = await _service.GetAllAsync(ct);
        return Ok(usuarios);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioDto>> GetById(Guid id, CancellationToken ct)
    {
        var usuario = await _service.GetByIdAsync(id, ct);
        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Create(CreateUsuarioRequest request, CancellationToken ct)
    {
        var usuario = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UsuarioDto>> Update(Guid id, UpdateUsuarioRequest request, CancellationToken ct)
    {
        var usuario = await _service.UpdateAsync(id, request, ct);
        return Ok(usuario);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    // Público de propósito (AllowAnonymous ignora o [Authorize(Roles="Admin")] da classe):
    // avatar de qualquer usuário precisa aparecer em telas que Editor/Viewer também acessam
    // (ex: feed de atividades), não só pra quem gerencia usuários.
    [HttpGet("{id:guid}/avatar")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvatar(Guid id, CancellationToken ct)
    {
        var arquivo = await _service.GetAvatarAsync(id, ct);
        return File(arquivo.Conteudo, arquivo.ContentType);
    }
}
