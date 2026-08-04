using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AtasFinanceiras.Application.Common.Exceptions;
using AtasFinanceiras.Application.DTOs.Categorias;
using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Application.Interfaces.Services;
using AtasFinanceiras.Domain.Entities;

namespace AtasFinanceiras.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly IUnitOfWork _uow;

    public CategoriaService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<CategoriaDto>> GetAllAsync(CancellationToken ct = default)
    {
        var categorias = await _uow.Categorias.GetAllAsync(ct);
        var dtos = new List<CategoriaDto>(categorias.Count);
        foreach (var categoria in categorias)
        {
            var count = await _uow.Categorias.CountAtasAsync(categoria.Id, ct);
            dtos.Add(ToDto(categoria, count));
        }
        return dtos;
    }

    public async Task<CategoriaDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _uow.Categorias.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Categoria), id);
        var count = await _uow.Categorias.CountAtasAsync(id, ct);
        return ToDto(categoria, count);
    }

    public async Task<CategoriaDto> CreateAsync(CreateCategoriaRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("O nome da categoria é obrigatório.");

        if (await _uow.Categorias.ExistsWithNameAsync(request.Name, ct: ct))
            throw new ConflictException($"Já existe uma categoria chamada '{request.Name}'.");

        var categoria = new Categoria
        {
            Name = request.Name,
            Slug = Slugify(request.Name),
            Description = request.Description,
            Icon = request.Icon,
            Color = request.Color,
            MostrarNoSite = request.MostrarNoSite,
            OrdemSite = request.OrdemSite,
        };

        await _uow.Categorias.AddAsync(categoria, ct);
        await _uow.SaveChangesAsync(ct);

        return ToDto(categoria, 0);
    }

    public async Task<CategoriaDto> UpdateAsync(Guid id, UpdateCategoriaRequest request, CancellationToken ct = default)
    {
        var categoria = await _uow.Categorias.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Categoria), id);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("O nome da categoria é obrigatório.");

        if (await _uow.Categorias.ExistsWithNameAsync(request.Name, id, ct))
            throw new ConflictException($"Já existe uma categoria chamada '{request.Name}'.");

        categoria.Name = request.Name;
        categoria.Slug = Slugify(request.Name);
        categoria.Description = request.Description;
        categoria.Icon = request.Icon;
        categoria.Color = request.Color;
        categoria.MostrarNoSite = request.MostrarNoSite;
        categoria.OrdemSite = request.OrdemSite;
        categoria.UpdatedAt = DateTime.UtcNow;

        _uow.Categorias.Update(categoria);
        await _uow.SaveChangesAsync(ct);

        var count = await _uow.Categorias.CountAtasAsync(id, ct);
        return ToDto(categoria, count);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _uow.Categorias.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Categoria), id);

        if (await _uow.Categorias.HasAtasAsync(id, ct))
            throw new ConflictException("Não é possível excluir uma categoria que possui atas vinculadas.");

        _uow.Categorias.Remove(categoria);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task UpdateOrdemSiteAsync(UpdateOrdemSiteRequest request, CancellationToken ct = default)
    {
        // Duas fases (zera tudo, depois aplica) — evita violar a constraint de unicidade
        // de OrdemSite quando duas categorias trocam de posição entre si na mesma chamada.
        var categorias = new List<Categoria>();
        foreach (var item in request.Itens)
        {
            var categoria = await _uow.Categorias.GetByIdAsync(item.CategoriaId, ct)
                ?? throw new NotFoundException(nameof(Categoria), item.CategoriaId);
            categorias.Add(categoria);
            categoria.OrdemSite = null;
        }
        await _uow.SaveChangesAsync(ct);

        for (var i = 0; i < request.Itens.Count; i++)
        {
            categorias[i].MostrarNoSite = request.Itens[i].MostrarNoSite;
            categorias[i].OrdemSite = request.Itens[i].OrdemSite;
        }
        await _uow.SaveChangesAsync(ct);
    }

    private static CategoriaDto ToDto(Categoria categoria, int atasCount) => new()
    {
        Id = categoria.Id,
        Name = categoria.Name,
        Slug = categoria.Slug,
        Description = categoria.Description,
        Icon = categoria.Icon,
        Color = categoria.Color,
        MostrarNoSite = categoria.MostrarNoSite,
        OrdemSite = categoria.OrdemSite,
        AtasCount = atasCount,
        CreatedAt = categoria.CreatedAt,
        UpdatedAt = categoria.UpdatedAt,
    };

    private static string Slugify(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
        return slug.Trim('-');
    }
}
