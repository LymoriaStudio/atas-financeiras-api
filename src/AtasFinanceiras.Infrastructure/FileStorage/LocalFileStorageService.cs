using AtasFinanceiras.Application.Common.Exceptions;
using AtasFinanceiras.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace AtasFinanceiras.Infrastructure.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IOptions<LocalFileStorageOptions> options)
    {
        _basePath = options.Value.BasePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<ArquivoArmazenado> SaveAsync(Stream conteudo, string nomeOriginal, CancellationToken ct = default)
    {
        var extensao = Path.GetExtension(nomeOriginal);
        var nomeArmazenado = $"{Guid.NewGuid()}{extensao}";
        var caminhoCompleto = Path.Combine(_basePath, nomeArmazenado);

        await using (var destino = File.Create(caminhoCompleto))
        {
            await conteudo.CopyToAsync(destino, ct);
        }

        return new ArquivoArmazenado
        {
            CaminhoArmazenamento = nomeArmazenado,
            TamanhoBytes = new FileInfo(caminhoCompleto).Length,
        };
    }

    public Task<Stream> OpenReadAsync(string caminhoArmazenamento, CancellationToken ct = default)
    {
        // caminhoArmazenamento sempre vem de uma entidade AtaArquivo já persistida
        // (nunca de entrada crua do usuário), então não há risco de path traversal aqui.
        var caminhoCompleto = Path.Combine(_basePath, caminhoArmazenamento);

        // O registro no banco pode sobreviver a um redeploy sem volume mesmo quando o
        // arquivo físico não sobrevive (disco efêmero) — nesse caso é um 404 de verdade,
        // não um erro interno, e não deve vazar o caminho físico do servidor na mensagem.
        if (!File.Exists(caminhoCompleto))
            throw new NotFoundException("Arquivo não encontrado no armazenamento (pode ter sido perdido em um redeploy sem volume persistente).");

        Stream stream = File.OpenRead(caminhoCompleto);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string caminhoArmazenamento, CancellationToken ct = default)
    {
        var caminhoCompleto = Path.Combine(_basePath, caminhoArmazenamento);
        if (File.Exists(caminhoCompleto))
            File.Delete(caminhoCompleto);
        return Task.CompletedTask;
    }
}
