namespace AtasFinanceiras.Application.DTOs.Categorias;

public class OrdemSiteItem
{
    public Guid CategoriaId { get; set; }
    public bool MostrarNoSite { get; set; }
    public int? OrdemSite { get; set; }
}

public class UpdateOrdemSiteRequest
{
    public List<OrdemSiteItem> Itens { get; set; } = new();
}
