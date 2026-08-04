using AtasFinanceiras.Application.DTOs.Atas;
using AtasFinanceiras.Application.DTOs.Common;

namespace AtasFinanceiras.Application.Interfaces.Services;

public class ArquivoParaUpload
{
    public Stream Conteudo { get; init; } = Stream.Null;
    public string NomeOriginal { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long TamanhoBytes { get; init; }
}

public class ArquivoParaDownload
{
    public Stream Conteudo { get; init; } = Stream.Null;
    public string Nome { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

public interface IAtaService
{
    Task<PagedResult<AtaDto>> SearchAsync(AtaFiltro filtro, CancellationToken ct = default);
    Task<AtaDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AtaDto> CreateAsync(CreateAtaRequest request, CancellationToken ct = default);
    Task<AtaDto> UpdateAsync(Guid id, UpdateAtaRequest request, CancellationToken ct = default);

    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task RestoreAsync(Guid id, CancellationToken ct = default);
    Task PurgeAsync(Guid id, CancellationToken ct = default);

    Task<AtaArquivoDto> AddArquivoAsync(Guid ataId, ArquivoParaUpload arquivo, CancellationToken ct = default);
    Task RemoveArquivoAsync(Guid ataId, Guid arquivoId, CancellationToken ct = default);
    Task<ArquivoParaDownload> DownloadArquivoAsync(Guid ataId, Guid arquivoId, CancellationToken ct = default);

    Task<AtaDto> RegistrarDownloadAsync(Guid id, CancellationToken ct = default);
}
