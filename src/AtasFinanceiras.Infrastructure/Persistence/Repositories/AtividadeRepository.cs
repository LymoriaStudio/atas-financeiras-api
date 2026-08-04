using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtasFinanceiras.Infrastructure.Persistence.Repositories;

public class AtividadeRepository : GenericRepository<Atividade>, IAtividadeRepository
{
    public AtividadeRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Atividade>> GetRecentesAsync(int limit, CancellationToken ct = default) =>
        await Context.Atividades
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
}
