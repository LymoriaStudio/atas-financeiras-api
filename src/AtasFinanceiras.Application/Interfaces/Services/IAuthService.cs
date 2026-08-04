using AtasFinanceiras.Application.DTOs.Auth;
using AtasFinanceiras.Application.DTOs.Usuarios;

namespace AtasFinanceiras.Application.Interfaces.Services;

public class AuthResult
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public UsuarioDto Usuario { get; init; } = null!;
}

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResult> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task LogoutAsync(Guid usuarioId, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid usuarioId, string senhaAtual, string novaSenha, CancellationToken ct = default);
}
