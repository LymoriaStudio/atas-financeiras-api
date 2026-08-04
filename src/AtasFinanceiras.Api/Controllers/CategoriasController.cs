using AtasFinanceiras.Application.DTOs.Categorias;
using AtasFinanceiras.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtasFinanceiras.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _service;

    public CategoriasController(ICategoriaService service)
    {
        _service = service;
    }

    // Leitura é pública: o portal (site institucional, sem login) precisa listar categorias.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> GetAll(CancellationToken ct)
    {
        var categorias = await _service.GetAllAsync(ct);
        return Ok(categorias);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoriaDto>> GetById(Guid id, CancellationToken ct)
    {
        var categoria = await _service.GetByIdAsync(id, ct);
        return Ok(categoria);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<CategoriaDto>> Create(CreateCategoriaRequest request, CancellationToken ct)
    {
        var categoria = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<CategoriaDto>> Update(Guid id, UpdateCategoriaRequest request, CancellationToken ct)
    {
        var categoria = await _service.UpdateAsync(id, request, ct);
        return Ok(categoria);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPut("ordem-site")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> UpdateOrdemSite(UpdateOrdemSiteRequest request, CancellationToken ct)
    {
        await _service.UpdateOrdemSiteAsync(request, ct);
        return NoContent();
    }
}
