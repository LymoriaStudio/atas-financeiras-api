using AtasFinanceiras.Domain.Entities;

namespace AtasFinanceiras.Application.Interfaces.Repositories;

public interface IAtividadeRepository : IGenericRepository<Atividade>
{
    Task<IReadOnlyList<Atividade>> GetRecentesAsync(int limit, CancellationToken ct = default);
}
