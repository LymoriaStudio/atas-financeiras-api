using AtasFinanceiras.Domain.Common;
using AtasFinanceiras.Domain.Enums;

namespace AtasFinanceiras.Domain.Entities;

public class Usuario : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();
    public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
}
