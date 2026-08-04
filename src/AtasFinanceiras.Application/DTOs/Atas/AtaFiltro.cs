namespace AtasFinanceiras.Application.DTOs.Atas;

public class AtaFiltro
{
    public string? Query { get; set; }
    public Guid? CategoriaId { get; set; }
    public string? Tipo { get; set; }
    public string? Status { get; set; }
    public int? Ano { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public bool IncluirExcluidas { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
