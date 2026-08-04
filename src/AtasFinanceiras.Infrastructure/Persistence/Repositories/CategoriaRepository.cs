using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtasFinanceiras.Infrastructure.Persistence.Repositories;

public class CategoriaRepository : GenericRepository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(AppDbContext context) : base(context) { }

    public async Task<Categoria?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await Context.Categorias.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public async Task<bool> ExistsWithNameAsync(string name, Guid? excludingId = null, CancellationToken ct = default)
    {
        var lowerName = name.ToLower();
        return await Context.Categorias.AnyAsync(
            c => c.Name.ToLower() == lowerName && (excludingId == null || c.Id != excludingId), ct);
    }

    public async Task<int> CountAtasAsync(Guid categoriaId, CancellationToken ct = default) =>
        await Context.Atas.CountAsync(a => a.CategoriaId == categoriaId && a.DeletedAt == null, ct);

    public async Task<bool> HasAtasAsync(Guid categoriaId, CancellationToken ct = default) =>
        await Context.Atas.AnyAsync(a => a.CategoriaId == categoriaId, ct);
}
