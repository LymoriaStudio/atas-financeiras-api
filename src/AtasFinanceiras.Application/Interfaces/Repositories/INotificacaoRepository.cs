using AtasFinanceiras.Domain.Entities;

namespace AtasFinanceiras.Application.Interfaces.Repositories;

public interface INotificacaoRepository : IGenericRepository<Notificacao>
{
    Task<Notificacao?> GetByAtividadeAndUsuarioAsync(Guid atividadeId, Guid usuarioId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetViewedAtividadeIdsAsync(Guid usuarioId, CancellationToken ct = default);
}
