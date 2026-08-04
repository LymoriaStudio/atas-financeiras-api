using AtasFinanceiras.Application.Interfaces.Repositories;

namespace AtasFinanceiras.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        IAtaRepository atas,
        ICategoriaRepository categorias,
        IUsuarioRepository usuarios,
        IAtividadeRepository atividades,
        INotificacaoRepository notificacoes)
    {
        _context = context;
        Atas = atas;
        Categorias = categorias;
        Usuarios = usuarios;
        Atividades = atividades;
        Notificacoes = notificacoes;
    }

    public IAtaRepository Atas { get; }
    public ICategoriaRepository Categorias { get; }
    public IUsuarioRepository Usuarios { get; }
    public IAtividadeRepository Atividades { get; }
    public INotificacaoRepository Notificacoes { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
