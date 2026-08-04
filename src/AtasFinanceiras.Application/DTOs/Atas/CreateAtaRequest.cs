namespace AtasFinanceiras.Application.DTOs.Atas;

public class CreateAtaRequest
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
    public string Status { get; set; } = "Rascunho";
    public Guid CategoriaId { get; set; }
}
