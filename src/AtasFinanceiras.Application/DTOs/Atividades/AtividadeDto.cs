namespace AtasFinanceiras.Application.DTOs.Atividades;

public class AtividadeDto
{
    public Guid Id { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UsuarioNome { get; set; }
    public string? UsuarioAvatarUrl { get; set; }
    public bool Viewed { get; set; }
}
