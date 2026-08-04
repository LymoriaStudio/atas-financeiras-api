namespace AtasFinanceiras.Application.DTOs.Categorias;

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool MostrarNoSite { get; set; }
    public int? OrdemSite { get; set; }
    public int AtasCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
