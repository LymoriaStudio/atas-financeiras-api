using AtasFinanceiras.Application.DTOs.Atividades;
using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Application.Interfaces.Services;
using AtasFinanceiras.Domain.Entities;

namespace AtasFinanceiras.Application.Services;

public class AtividadeService : IAtividadeService
{
    private readonly IUnitOfWork _uow;

    public AtividadeService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task RegistrarAsync(Guid? usuarioId, string acao, string? documento, CancellationToken ct = default)
    {
        var atividade = new Atividade
        {
            UsuarioId = usuarioId,
            Acao = acao,
            Documento = documento,
        };
        await _uow.Atividades.AddAsync(atividade, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AtividadeDto>> GetRecentesAsync(Guid? usuarioLogadoId, int limit, CancellationToken ct = default)
    {
        var atividades = await _uow.Atividades.GetRecentesAsync(limit, ct);

        var visualizadas = usuarioLogadoId is null
            ? new HashSet<Guid>()
            : (await _uow.Notificacoes.GetViewedAtividadeIdsAsync(usuarioLogadoId.Value, ct)).ToHashSet();

        return atividades.Select(a => new AtividadeDto
        {
            Id = a.Id,
            Acao = a.Acao,
            Documento = a.Documento,
            CreatedAt = a.CreatedAt,
            UsuarioNome = a.Usuario?.FullName,
            UsuarioAvatarUrl = a.Usuario?.AvatarUrl,
            Viewed = visualizadas.Contains(a.Id),
        }).ToList();
    }

    public async Task ToggleNotificacaoVisualizadaAsync(Guid usuarioId, Guid atividadeId, bool viewed, CancellationToken ct = default)
    {
        var notificacao = await _uow.Notificacoes.GetByAtividadeAndUsuarioAsync(atividadeId, usuarioId, ct);
        if (notificacao is null)
        {
            notificacao = new Notificacao
            {
                AtividadeId = atividadeId,
                UsuarioId = usuarioId,
                Viewed = viewed,
                ViewedAt = viewed ? DateTime.UtcNow : null,
            };
            await _uow.Notificacoes.AddAsync(notificacao, ct);
        }
        else
        {
            notificacao.Viewed = viewed;
            notificacao.ViewedAt = viewed ? DateTime.UtcNow : null;
            notificacao.UpdatedAt = DateTime.UtcNow;
            _uow.Notificacoes.Update(notificacao);
        }

        await _uow.SaveChangesAsync(ct);
    }
}
