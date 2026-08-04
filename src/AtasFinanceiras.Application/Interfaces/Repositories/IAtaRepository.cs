using AtasFinanceiras.Application.DTOs.Atas;
using AtasFinanceiras.Domain.Entities;

namespace AtasFinanceiras.Application.Interfaces.Repositories;

public interface IAtaRepository : IGenericRepository<Ata>
{
    Task<Ata?> GetByIdWithArquivosAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Ata> Items, int TotalCount)> SearchAsync(AtaFiltro filtro, CancellationToken ct = default);
    Task<AtaArquivo?> GetArquivoAsync(Guid ataId, Guid arquivoId, CancellationToken ct = default);
    void AddArquivo(AtaArquivo arquivo);
    void RemoveArquivo(AtaArquivo arquivo);
}
