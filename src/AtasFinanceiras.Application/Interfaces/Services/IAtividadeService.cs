using AtasFinanceiras.Application.DTOs.Atividades;

namespace AtasFinanceiras.Application.Interfaces.Services;

public interface IAtividadeService
{
    Task RegistrarAsync(Guid? usuarioId, string acao, string? documento, CancellationToken ct = default);
    Task<IReadOnlyList<AtividadeDto>> GetRecentesAsync(Guid? usuarioLogadoId, int limit, CancellationToken ct = default);
    Task ToggleNotificacaoVisualizadaAsync(Guid usuarioId, Guid atividadeId, bool viewed, CancellationToken ct = default);
}
