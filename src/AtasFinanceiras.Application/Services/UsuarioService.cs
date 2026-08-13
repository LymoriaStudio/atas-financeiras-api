using AtasFinanceiras.Application.Common.Exceptions;
using AtasFinanceiras.Application.DTOs.Usuarios;
using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Application.Interfaces.Services;
using AtasFinanceiras.Domain.Entities;
using AtasFinanceiras.Domain.Enums;

namespace AtasFinanceiras.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IFileStorageService _fileStorage;

    public UsuarioService(IUnitOfWork uow, IPasswordHasher passwordHasher, IFileStorageService fileStorage)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _fileStorage = fileStorage;
    }

    public async Task<IReadOnlyList<UsuarioDto>> GetAllAsync(CancellationToken ct = default)
    {
        var usuarios = await _uow.Usuarios.GetAllAsync(ct);
        return usuarios.Select(ToDto).ToList();
    }

    public async Task<UsuarioDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Usuario), id);
        return ToDto(usuario);
    }

    public async Task<UsuarioDto> CreateAsync(CreateUsuarioRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Nome e e-mail são obrigatórios.");

        if (request.Password.Length < 8)
            throw new ValidationException("A senha deve ter no mínimo 8 caracteres.");

        if (await _uow.Usuarios.ExistsWithEmailAsync(request.Email, ct: ct))
            throw new ConflictException($"Já existe um usuário com o e-mail '{request.Email}'.");

        var usuario = new Usuario
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = ParseRole(request.Role),
            JobTitle = request.JobTitle,
            Department = request.Department,
            IsActive = request.IsActive,
        };

        await _uow.Usuarios.AddAsync(usuario, ct);
        await _uow.SaveChangesAsync(ct);

        return ToDto(usuario);
    }

    public async Task<UsuarioDto> UpdateAsync(Guid id, UpdateUsuarioRequest request, CancellationToken ct = default)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Usuario), id);

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Nome e e-mail são obrigatórios.");

        if (await _uow.Usuarios.ExistsWithEmailAsync(request.Email, id, ct))
            throw new ConflictException($"Já existe um usuário com o e-mail '{request.Email}'.");

        usuario.FullName = request.FullName;
        usuario.Email = request.Email;
        usuario.Role = ParseRole(request.Role);
        usuario.JobTitle = request.JobTitle;
        usuario.Department = request.Department;
        // AvatarUrl não é tocado aqui de propósito — só é alterado via UpdateAvatarAsync,
        // senão qualquer edição de perfil sobrescreveria a chave de armazenamento do avatar
        // com a URL pública já construída (formato incompatível com o que o storage espera).
        usuario.IsActive = request.IsActive;
        usuario.UpdatedAt = DateTime.UtcNow;

        _uow.Usuarios.Update(usuario);
        await _uow.SaveChangesAsync(ct);

        return ToDto(usuario);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Usuario), id);
        _uow.Usuarios.Remove(usuario);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<UsuarioDto> UpdateAvatarAsync(Guid id, ArquivoParaUpload arquivo, CancellationToken ct = default)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Usuario), id);

        var salvo = await _fileStorage.SaveAsync(arquivo.Conteudo, arquivo.NomeOriginal, ct);

        // Apaga o avatar antigo do disco (se houver) pra não acumular arquivo órfão a cada troca.
        var avatarAnterior = usuario.AvatarUrl;

        usuario.AvatarUrl = salvo.CaminhoArmazenamento;
        usuario.UpdatedAt = DateTime.UtcNow;
        _uow.Usuarios.Update(usuario);
        await _uow.SaveChangesAsync(ct);

        if (!string.IsNullOrEmpty(avatarAnterior))
            await _fileStorage.DeleteAsync(avatarAnterior, ct);

        return ToDto(usuario);
    }

    public async Task<ArquivoParaDownload> GetAvatarAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Usuario), id);

        if (string.IsNullOrEmpty(usuario.AvatarUrl))
            throw new NotFoundException("Avatar", id);

        var stream = await _fileStorage.OpenReadAsync(usuario.AvatarUrl, ct);

        return new ArquivoParaDownload
        {
            Conteudo = stream,
            Nome = $"avatar{Path.GetExtension(usuario.AvatarUrl)}",
            ContentType = GuessImageContentType(usuario.AvatarUrl),
        };
    }

    private static string GuessImageContentType(string caminhoArmazenamento) =>
        Path.GetExtension(caminhoArmazenamento).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream",
        };

    private static UserRole ParseRole(string role) =>
        Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed)
            ? parsed
            : throw new ValidationException($"Perfil de acesso inválido: '{role}'.");

    private static UsuarioDto ToDto(Usuario usuario) => new()
    {
        Id = usuario.Id,
        FullName = usuario.FullName,
        Email = usuario.Email,
        Role = usuario.Role.ToString().ToLowerInvariant(),
        JobTitle = usuario.JobTitle,
        Department = usuario.Department,
        AvatarUrl = string.IsNullOrEmpty(usuario.AvatarUrl) ? null : $"/api/usuarios/{usuario.Id}/avatar",
        IsActive = usuario.IsActive,
        CreatedAt = usuario.CreatedAt,
        UpdatedAt = usuario.UpdatedAt,
    };
}
