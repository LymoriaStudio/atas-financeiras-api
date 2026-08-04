using AtasFinanceiras.Application.Common.Exceptions;
using AtasFinanceiras.Application.DTOs.Auth;
using AtasFinanceiras.Application.DTOs.Usuarios;
using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Application.Interfaces.Services;
using AtasFinanceiras.Domain.Entities;

namespace AtasFinanceiras.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUnitOfWork uow, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await _uow.Usuarios.GetByEmailAsync(request.Email, ct);
        if (usuario is null || !usuario.IsActive || !_passwordHasher.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedAppException("E-mail ou senha incorretos.");

        return await IssueTokensAsync(usuario, ct);
    }

    public async Task<AuthResult> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var usuario = await _uow.Usuarios.GetByRefreshTokenAsync(request.RefreshToken, ct);
        if (usuario is null || !usuario.IsActive ||
            usuario.RefreshTokenExpiresAt is null || usuario.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAppException("Refresh token inválido ou expirado.");
        }

        return await IssueTokensAsync(usuario, ct);
    }

    public async Task LogoutAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(usuarioId, ct);
        if (usuario is null) return;

        usuario.RefreshToken = null;
        usuario.RefreshTokenExpiresAt = null;
        _uow.Usuarios.Update(usuario);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(Guid usuarioId, string senhaAtual, string novaSenha, CancellationToken ct = default)
    {
        if (novaSenha.Length < 8)
            throw new ValidationException("A senha deve ter no mínimo 8 caracteres.");

        var usuario = await _uow.Usuarios.GetByIdAsync(usuarioId, ct)
            ?? throw new NotFoundException(nameof(Usuario), usuarioId);

        if (!_passwordHasher.Verify(senhaAtual, usuario.PasswordHash))
            throw new UnauthorizedAppException("Senha atual incorreta.");

        usuario.PasswordHash = _passwordHasher.Hash(novaSenha);
        usuario.UpdatedAt = DateTime.UtcNow;
        _uow.Usuarios.Update(usuario);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<AuthResult> IssueTokensAsync(Usuario usuario, CancellationToken ct)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(usuario);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        usuario.RefreshToken = refreshToken;
        usuario.RefreshTokenExpiresAt = DateTime.UtcNow.Add(_jwtTokenService.RefreshTokenLifetime);
        _uow.Usuarios.Update(usuario);
        await _uow.SaveChangesAsync(ct);

        return new AuthResult
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshToken = refreshToken,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                FullName = usuario.FullName,
                Email = usuario.Email,
                Role = usuario.Role.ToString().ToLowerInvariant(),
                JobTitle = usuario.JobTitle,
                Department = usuario.Department,
                AvatarUrl = usuario.AvatarUrl,
                IsActive = usuario.IsActive,
                CreatedAt = usuario.CreatedAt,
                UpdatedAt = usuario.UpdatedAt,
            },
        };
    }
}
