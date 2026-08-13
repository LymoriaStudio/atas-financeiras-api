using AtasFinanceiras.Application.DTOs.Usuarios;

namespace AtasFinanceiras.Application.Interfaces.Services;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioDto>> GetAllAsync(CancellationToken ct = default);
    Task<UsuarioDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UsuarioDto> CreateAsync(CreateUsuarioRequest request, CancellationToken ct = default);
    Task<UsuarioDto> UpdateAsync(Guid id, UpdateUsuarioRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<UsuarioDto> UpdateAvatarAsync(Guid id, ArquivoParaUpload arquivo, CancellationToken ct = default);
    Task<ArquivoParaDownload> GetAvatarAsync(Guid id, CancellationToken ct = default);
}
