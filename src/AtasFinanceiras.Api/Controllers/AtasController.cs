using AtasFinanceiras.Application.DTOs.Atas;
using AtasFinanceiras.Application.DTOs.Common;
using AtasFinanceiras.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace AtasFinanceiras.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AtasController : ControllerBase
{
    private readonly IAtaService _service;
    private readonly ICurrentUserService _currentUser;

    public AtasController(IAtaService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // Sem login (portal público): só enxerga atas publicadas e não excluídas,
    // independentemente do que for pedido na query string.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<AtaDto>>> Search([FromQuery] AtaFiltro filtro, CancellationToken ct)
    {
        if (_currentUser.UserId is null)
        {
            filtro.Status = "Publicado";
            filtro.IncluirExcluidas = false;
        }

        var result = await _service.SearchAsync(filtro, ct);
        return Ok(result);
    }

    [HttpGet("lixeira")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<AtaDto>>> Lixeira([FromQuery] AtaFiltro filtro, CancellationToken ct)
    {
        filtro.IncluirExcluidas = true;
        var result = await _service.SearchAsync(filtro, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<AtaDto>> GetById(Guid id, CancellationToken ct)
    {
        var ata = await _service.GetByIdAsync(id, ct);

        if (_currentUser.UserId is null && (ata.Status != "Publicado" || ata.DeletedAt != null))
            return NotFound();

        return Ok(ata);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<AtaDto>> Create(CreateAtaRequest request, CancellationToken ct)
    {
        var ata = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = ata.Id }, ata);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<AtaDto>> Update(Guid id, UpdateAtaRequest request, CancellationToken ct)
    {
        var ata = await _service.UpdateAsync(id, request, ct);
        return Ok(ata);
    }

    // Soft delete (lixeira) — restrito a Admin, conforme matriz de permissões.
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken ct)
    {
        await _service.SoftDeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/restaurar")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AtaDto>> Restore(Guid id, CancellationToken ct)
    {
        await _service.RestoreAsync(id, ct);
        var ata = await _service.GetByIdAsync(id, ct);
        return Ok(ata);
    }

    [HttpDelete("{id:guid}/definitivo")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Purge(Guid id, CancellationToken ct)
    {
        await _service.PurgeAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/arquivos")]
    [Authorize(Roles = "Admin,Editor")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<AtaArquivoDto>> UploadArquivo(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest(new { detail = "Arquivo vazio." });

        await using var stream = file.OpenReadStream();
        var arquivo = await _service.AddArquivoAsync(id, new ArquivoParaUpload
        {
            Conteudo = stream,
            NomeOriginal = file.FileName,
            ContentType = file.ContentType,
            TamanhoBytes = file.Length,
        }, ct);

        return Ok(arquivo);
    }

    [HttpDelete("{id:guid}/arquivos/{arquivoId:guid}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> RemoveArquivo(Guid id, Guid arquivoId, CancellationToken ct)
    {
        await _service.RemoveArquivoAsync(id, arquivoId, ct);
        return NoContent();
    }

    // Download é público (mesmo fluxo do site institucional hoje), só exige que o arquivo exista.
    // Por padrão serve "inline" (pra abrir dentro de um <iframe>/nova aba); só força o
    // download ("Salvar como") quando ?download=true é passado explicitamente — usar o
    // File(stream, contentType, fileDownloadName) sempre manda Content-Disposition:attachment,
    // o que quebrava a pré-visualização em iframe (o navegador tentava baixar em vez de exibir).
    [HttpGet("{id:guid}/arquivos/{arquivoId:guid}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadArquivo(Guid id, Guid arquivoId, [FromQuery] bool download, CancellationToken ct)
    {
        var arquivo = await _service.DownloadArquivoAsync(id, arquivoId, ct);

        var contentDisposition = new ContentDispositionHeaderValue(download ? "attachment" : "inline")
        {
            FileNameStar = arquivo.Nome,
        };
        Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();

        return File(arquivo.Conteudo, arquivo.ContentType);
    }

    [HttpPost("{id:guid}/download")]
    [AllowAnonymous]
    public async Task<ActionResult<AtaDto>> RegistrarDownload(Guid id, CancellationToken ct)
    {
        var ata = await _service.RegistrarDownloadAsync(id, ct);
        return Ok(ata);
    }
}
