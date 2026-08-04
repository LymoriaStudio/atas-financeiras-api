using AtasFinanceiras.Domain.Common;
using AtasFinanceiras.Domain.Enums;

namespace AtasFinanceiras.Domain.Entities;

public class Ata : AuditableEntity
{
    public string Numero { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Tipo { get; set; }
    public string? Descricao { get; set; }
    public DateOnly Data { get; set; }
    public TimeOnly? Horario { get; set; }
    public string? Local { get; set; }
    public string Presidente { get; set; } = string.Empty;
    public string? Secretario { get; set; }
    public string? Participantes { get; set; }
    public AtaStatus Status { get; set; } = AtaStatus.Rascunho;
    public int DownloadsCount { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Guid CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public ICollection<AtaArquivo> Arquivos { get; set; } = new List<AtaArquivo>();
}
