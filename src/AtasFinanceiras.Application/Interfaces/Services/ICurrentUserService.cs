using AtasFinanceiras.Domain.Enums;

namespace AtasFinanceiras.Application.Interfaces.Services;

// Abstrai a leitura do usuário autenticado a partir do HttpContext,
// pra Application não precisar referenciar ASP.NET Core diretamente.
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
}
