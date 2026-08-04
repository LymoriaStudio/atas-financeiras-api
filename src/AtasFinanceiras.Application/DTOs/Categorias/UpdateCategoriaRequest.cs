namespace AtasFinanceiras.Application.DTOs.Categorias;

public class UpdateCategoriaRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool MostrarNoSite { get; set; } = true;
    public int? OrdemSite { get; set; }
}
