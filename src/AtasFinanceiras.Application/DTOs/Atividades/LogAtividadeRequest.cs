namespace AtasFinanceiras.Application.DTOs.Atividades;

public class LogAtividadeRequest
{
    public string Acao { get; set; } = string.Empty;
    public string? Documento { get; set; }
}
