namespace AtasFinanceiras.Application.Interfaces.Services;

public class ArquivoArmazenado
{
    public string CaminhoArmazenamento { get; init; } = string.Empty;
    public long TamanhoBytes { get; init; }
}

// Abstrai onde/como os arquivos ficam gravados. A implementação inicial usa disco local
// (volume do Railway); trocar para outra estratégia de armazenamento no futuro
// (ex: pasta de rede do cliente) não deve afetar quem consome esta interface.
public interface IFileStorageService
{
    Task<ArquivoArmazenado> SaveAsync(Stream conteudo, string nomeOriginal, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string caminhoArmazenamento, CancellationToken ct = default);
    Task DeleteAsync(string caminhoArmazenamento, CancellationToken ct = default);
}
