using AtasFinanceiras.Application.DTOs.Categorias;

namespace AtasFinanceiras.Application.Interfaces.Services;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaDto>> GetAllAsync(CancellationToken ct = default);
    Task<CategoriaDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CategoriaDto> CreateAsync(CreateCategoriaRequest request, CancellationToken ct = default);
    Task<CategoriaDto> UpdateAsync(Guid id, UpdateCategoriaRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task UpdateOrdemSiteAsync(UpdateOrdemSiteRequest request, CancellationToken ct = default);
}
