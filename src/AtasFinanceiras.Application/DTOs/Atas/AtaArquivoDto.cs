namespace AtasFinanceiras.Application.DTOs.Atas;

public class AtaArquivoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Preenchido pela API a partir da rota de download; nunca aponta direto pro storage físico.
    public string DownloadUrl { get; set; } = string.Empty;
}
