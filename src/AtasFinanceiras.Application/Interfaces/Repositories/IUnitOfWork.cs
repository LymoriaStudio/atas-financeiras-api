namespace AtasFinanceiras.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    IAtaRepository Atas { get; }
    ICategoriaRepository Categorias { get; }
    IUsuarioRepository Usuarios { get; }
    IAtividadeRepository Atividades { get; }
    INotificacaoRepository Notificacoes { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
