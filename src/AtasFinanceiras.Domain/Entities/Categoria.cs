using AtasFinanceiras.Domain.Common;

namespace AtasFinanceiras.Domain.Entities;

public class Categoria : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool MostrarNoSite { get; set; } = true;
    public int? OrdemSite { get; set; }

    public ICollection<Ata> Atas { get; set; } = new List<Ata>();
}
