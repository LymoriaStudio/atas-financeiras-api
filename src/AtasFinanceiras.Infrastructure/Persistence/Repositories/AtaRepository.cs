using AtasFinanceiras.Application.DTOs.Atas;
using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Domain.Entities;
using AtasFinanceiras.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtasFinanceiras.Infrastructure.Persistence.Repositories;

public class AtaRepository : GenericRepository<Ata>, IAtaRepository
{
    public AtaRepository(AppDbContext context) : base(context) { }

    public async Task<Ata?> GetByIdWithArquivosAsync(Guid id, CancellationToken ct = default) =>
        await Context.Atas
            .Include(a => a.Arquivos)
            .Include(a => a.Categoria)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<(IReadOnlyList<Ata> Items, int TotalCount)> SearchAsync(AtaFiltro filtro, CancellationToken ct = default)
    {
        var query = Context.Atas
            .Include(a => a.Categoria)
            .Include(a => a.Arquivos)
            .AsQueryable();

        query = filtro.IncluirExcluidas
            ? query.Where(a => a.DeletedAt != null)
            : query.Where(a => a.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(filtro.Query))
        {
            var q = filtro.Query.Trim().ToLower();
            query = query.Where(a => a.Titulo.ToLower().Contains(q) || a.Numero.ToLower().Contains(q));
        }

        if (filtro.CategoriaId.HasValue)
            query = query.Where(a => a.CategoriaId == filtro.CategoriaId.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Tipo))
            query = query.Where(a => a.Tipo == filtro.Tipo);

        if (!string.IsNullOrWhiteSpace(filtro.Status) && Enum.TryParse<AtaStatus>(filtro.Status, ignoreCase: true, out var status))
            query = query.Where(a => a.Status == status);

        if (filtro.Ano.HasValue)
            query = query.Where(a => a.Data.Year == filtro.Ano.Value);

        if (filtro.DataInicio.HasValue)
            query = query.Where(a => a.Data >= filtro.DataInicio.Value);

        if (filtro.DataFim.HasValue)
            query = query.Where(a => a.Data <= filtro.DataFim.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.Data)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<AtaArquivo?> GetArquivoAsync(Guid ataId, Guid arquivoId, CancellationToken ct = default) =>
        await Context.AtaArquivos.FirstOrDefaultAsync(a => a.AtaId == ataId && a.Id == arquivoId, ct);

    public void AddArquivo(AtaArquivo arquivo) => Context.AtaArquivos.Add(arquivo);

    public void RemoveArquivo(AtaArquivo arquivo) => Context.AtaArquivos.Remove(arquivo);
}
