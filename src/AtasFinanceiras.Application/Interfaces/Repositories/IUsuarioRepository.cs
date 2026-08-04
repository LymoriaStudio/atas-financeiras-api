using AtasFinanceiras.Domain.Entities;

namespace AtasFinanceiras.Application.Interfaces.Repositories;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(string email, Guid? excludingId = null, CancellationToken ct = default);
}
