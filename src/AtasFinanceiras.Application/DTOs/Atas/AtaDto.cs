namespace AtasFinanceiras.Application.DTOs.Atas;

public class AtaDto
{
    public Guid Id { get; set; }
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
    public string Status { get; set; } = string.Empty;
    public int DownloadsCount { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Guid CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;

    public List<AtaArquivoDto> Arquivos { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
