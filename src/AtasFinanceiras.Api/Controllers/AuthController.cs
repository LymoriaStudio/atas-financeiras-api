using AtasFinanceiras.Application.DTOs.Auth;
using AtasFinanceiras.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtasFinanceiras.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUsuarioService _usuarioService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, IUsuarioService usuarioService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _usuarioService = usuarioService;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _authService.LogoutAsync(_currentUser.UserId!.Value, ct);
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(_currentUser.UserId!.Value, request.SenhaAtual, request.NovaSenha, ct);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var usuario = await _usuarioService.GetByIdAsync(_currentUser.UserId!.Value, ct);
        return Ok(usuario);
    }

    // Autoatendimento: qualquer usuário logado troca a própria foto (não é ação de Admin).
    [HttpPost("avatar")]
    [Authorize]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest(new { detail = "Arquivo vazio." });

        await using var stream = file.OpenReadStream();
        var usuario = await _usuarioService.UpdateAvatarAsync(_currentUser.UserId!.Value, new ArquivoParaUpload
        {
            Conteudo = stream,
            NomeOriginal = file.FileName,
            ContentType = file.ContentType,
            TamanhoBytes = file.Length,
        }, ct);

        return Ok(usuario);
    }
}
