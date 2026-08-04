using AtasFinanceiras.Domain.Common;

namespace AtasFinanceiras.Domain.Entities;

public class Atividade : BaseEntity
{
    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Acao { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
}
