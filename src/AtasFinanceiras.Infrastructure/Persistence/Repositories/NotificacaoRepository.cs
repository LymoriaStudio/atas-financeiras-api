using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtasFinanceiras.Infrastructure.Persistence.Repositories;

public class NotificacaoRepository : GenericRepository<Notificacao>, INotificacaoRepository
{
    public NotificacaoRepository(AppDbContext context) : base(context) { }

    public async Task<Notificacao?> GetByAtividadeAndUsuarioAsync(Guid atividadeId, Guid usuarioId, CancellationToken ct = default) =>
        await Context.Notificacoes.FirstOrDefaultAsync(n => n.AtividadeId == atividadeId && n.UsuarioId == usuarioId, ct);

    public async Task<IReadOnlyList<Guid>> GetViewedAtividadeIdsAsync(Guid usuarioId, CancellationToken ct = default) =>
        await Context.Notificacoes
            .Where(n => n.UsuarioId == usuarioId && n.Viewed)
            .Select(n => n.AtividadeId)
            .ToListAsync(ct);
}
