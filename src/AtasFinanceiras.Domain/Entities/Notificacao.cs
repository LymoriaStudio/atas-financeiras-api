using AtasFinanceiras.Domain.Common;

namespace AtasFinanceiras.Domain.Entities;

public class Notificacao : BaseEntity
{
    public Guid AtividadeId { get; set; }
    public Atividade Atividade { get; set; } = null!;

    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public bool Viewed { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
