using AtasFinanceiras.Domain.Common;

namespace AtasFinanceiras.Domain.Entities;

public class AtaArquivo : BaseEntity
{
    public Guid AtaId { get; set; }
    public Ata Ata { get; set; } = null!;

    public string Nome { get; set; } = string.Empty;
    public string CaminhoArmazenamento { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
