using AtasFinanceiras.Domain.Entities;

namespace AtasFinanceiras.Application.Interfaces.Repositories;

public interface ICategoriaRepository : IGenericRepository<Categoria>
{
    Task<Categoria?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> ExistsWithNameAsync(string name, Guid? excludingId = null, CancellationToken ct = default);
    Task<int> CountAtasAsync(Guid categoriaId, CancellationToken ct = default);
    Task<bool> HasAtasAsync(Guid categoriaId, CancellationToken ct = default);
}
