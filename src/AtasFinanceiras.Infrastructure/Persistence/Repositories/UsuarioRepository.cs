using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtasFinanceiras.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context) { }

    public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var lowerEmail = email.ToLower();
        return await Context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == lowerEmail, ct);
    }

    public async Task<Usuario?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default) =>
        await Context.Usuarios.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);

    public async Task<bool> ExistsWithEmailAsync(string email, Guid? excludingId = null, CancellationToken ct = default)
    {
        var lowerEmail = email.ToLower();
        return await Context.Usuarios.AnyAsync(
            u => u.Email.ToLower() == lowerEmail && (excludingId == null || u.Id != excludingId), ct);
    }
}
