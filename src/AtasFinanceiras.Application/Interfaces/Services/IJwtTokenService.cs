using AtasFinanceiras.Domain.Entities;

namespace AtasFinanceiras.Application.Interfaces.Services;

public class TokenGerado
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}

public interface IJwtTokenService
{
    TokenGerado GenerateAccessToken(Usuario usuario);
    string GenerateRefreshToken();
    TimeSpan RefreshTokenLifetime { get; }
}
