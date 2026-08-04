using AtasFinanceiras.Application.Common.Exceptions;
using AtasFinanceiras.Application.DTOs.Atas;
using AtasFinanceiras.Application.DTOs.Common;
using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Application.Interfaces.Services;
using AtasFinanceiras.Domain.Entities;
using AtasFinanceiras.Domain.Enums;

namespace AtasFinanceiras.Application.Services;

public class AtaService : IAtaService
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _fileStorage;

    public AtaService(IUnitOfWork uow, IFileStorageService fileStorage)
    {
        _uow = uow;
        _fileStorage = fileStorage;
    }

    public async Task<PagedResult<AtaDto>> SearchAsync(AtaFiltro filtro, CancellationToken ct = default)
    {
        var (items, total) = await _uow.Atas.SearchAsync(filtro, ct);
        return new PagedResult<AtaDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = filtro.Page,
            PageSize = filtro.PageSize,
            TotalCount = total,
        };
    }

    public async Task<AtaDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var ata = await _uow.Atas.GetByIdWithArquivosAsync(id, ct)
            ?? throw new NotFoundException(nameof(Ata), id);
        return ToDto(ata);
    }

    public async Task<AtaDto> CreateAsync(CreateAtaRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request.Numero, request.Titulo, request.Presidente, request.CategoriaId, ct);

        var ata = new Ata
        {
            Numero = request.Numero,
            Titulo = request.Titulo,
            Tipo = request.Tipo,
            Descricao = request.Descricao,
            Data = request.Data,
            Horario = request.Horario,
            Local = request.Local,
            Presidente = request.Presidente,
            Secretario = request.Secretario,
            Participantes = request.Participantes,
            Status = ParseStatus(request.Status),
            CategoriaId = request.CategoriaId,
        };

        await _uow.Atas.AddAsync(ata, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(ata.Id, ct);
    }

    public async Task<AtaDto> UpdateAsync(Guid id, UpdateAtaRequest request, CancellationToken ct = default)
    {
        var ata = await _uow.Atas.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Ata), id);

        await ValidateAsync(request.Numero, request.Titulo, request.Presidente, request.CategoriaId, ct);

        ata.Numero = request.Numero;
        ata.Titulo = request.Titulo;
        ata.Tipo = request.Tipo;
        ata.Descricao = request.Descricao;
        ata.Data = request.Data;
        ata.Horario = request.Horario;
        ata.Local = request.Local;
        ata.Presidente = request.Presidente;
        ata.Secretario = request.Secretario;
        ata.Participantes = request.Participantes;
        ata.Status = ParseStatus(request.Status);
        ata.CategoriaId = request.CategoriaId;
        ata.UpdatedAt = DateTime.UtcNow;

        _uow.Atas.Update(ata);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var ata = await _uow.Atas.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Ata), id);
        ata.DeletedAt = DateTime.UtcNow;
        _uow.Atas.Update(ata);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct = default)
    {
        var ata = await _uow.Atas.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Ata), id);
        ata.DeletedAt = null;
        _uow.Atas.Update(ata);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task PurgeAsync(Guid id, CancellationToken ct = default)
    {
        var ata = await _uow.Atas.GetByIdWithArquivosAsync(id, ct) ?? throw new NotFoundException(nameof(Ata), id);

        foreach (var arquivo in ata.Arquivos)
            await _fileStorage.DeleteAsync(arquivo.CaminhoArmazenamento, ct);

        _uow.Atas.Remove(ata);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<AtaArquivoDto> AddArquivoAsync(Guid ataId, ArquivoParaUpload arquivo, CancellationToken ct = default)
    {
        var ata = await _uow.Atas.GetByIdAsync(ataId, ct) ?? throw new NotFoundException(nameof(Ata), ataId);

        var salvo = await _fileStorage.SaveAsync(arquivo.Conteudo, arquivo.NomeOriginal, ct);

        var ataArquivo = new AtaArquivo
        {
            AtaId = ata.Id,
            Nome = arquivo.NomeOriginal,
            CaminhoArmazenamento = salvo.CaminhoArmazenamento,
            ContentType = arquivo.ContentType,
            TamanhoBytes = salvo.TamanhoBytes,
        };

        // ata já está rastreada pelo change tracker (veio de GetByIdAsync) — só mudar a
        // propriedade escalar já basta pro EF detectar. Adicionar o arquivo direto pelo
        // repositório (em vez de via ata.Arquivos + Update(ata)) evita o EF confundir a
        // linha nova com uma existente: como o Id é um Guid gerado aqui no C# (não pelo
        // banco), Update() no grafo tratava o arquivo novo como "Modified" e tentava um
        // UPDATE numa linha que ainda não existe — daí o erro de concorrência otimista.
        ata.UpdatedAt = DateTime.UtcNow;
        _uow.Atas.AddArquivo(ataArquivo);
        await _uow.SaveChangesAsync(ct);

        return ToArquivoDto(ataArquivo);
    }

    public async Task RemoveArquivoAsync(Guid ataId, Guid arquivoId, CancellationToken ct = default)
    {
        var arquivo = await _uow.Atas.GetArquivoAsync(ataId, arquivoId, ct)
            ?? throw new NotFoundException(nameof(AtaArquivo), arquivoId);

        await _fileStorage.DeleteAsync(arquivo.CaminhoArmazenamento, ct);

        _uow.Atas.RemoveArquivo(arquivo);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<ArquivoParaDownload> DownloadArquivoAsync(Guid ataId, Guid arquivoId, CancellationToken ct = default)
    {
        var arquivo = await _uow.Atas.GetArquivoAsync(ataId, arquivoId, ct)
            ?? throw new NotFoundException(nameof(AtaArquivo), arquivoId);

        var stream = await _fileStorage.OpenReadAsync(arquivo.CaminhoArmazenamento, ct);

        return new ArquivoParaDownload
        {
            Conteudo = stream,
            Nome = arquivo.Nome,
            ContentType = arquivo.ContentType,
        };
    }

    public async Task<AtaDto> RegistrarDownloadAsync(Guid id, CancellationToken ct = default)
    {
        var ata = await _uow.Atas.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Ata), id);
        ata.DownloadsCount += 1;
        _uow.Atas.Update(ata);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    private async Task ValidateAsync(string numero, string titulo, string presidente, Guid categoriaId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ValidationException("O número da ata é obrigatório.");
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ValidationException("O título da ata é obrigatório.");
        if (string.IsNullOrWhiteSpace(presidente))
            throw new ValidationException("O presidente/moderador da reunião é obrigatório.");

        var categoriaExiste = await _uow.Categorias.GetByIdAsync(categoriaId, ct);
        if (categoriaExiste is null)
            throw new ValidationException("A categoria informada não existe.");
    }

    private static AtaStatus ParseStatus(string status) =>
        Enum.TryParse<AtaStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : throw new ValidationException($"Status inválido: '{status}'.");

    private static AtaDto ToDto(Ata ata) => new()
    {
        Id = ata.Id,
        Numero = ata.Numero,
        Titulo = ata.Titulo,
        Tipo = ata.Tipo,
        Descricao = ata.Descricao,
        Data = ata.Data,
        Horario = ata.Horario,
        Local = ata.Local,
        Presidente = ata.Presidente,
        Secretario = ata.Secretario,
        Participantes = ata.Participantes,
        Status = ata.Status.ToString(),
        DownloadsCount = ata.DownloadsCount,
        DeletedAt = ata.DeletedAt,
        CategoriaId = ata.CategoriaId,
        CategoriaNome = ata.Categoria?.Name ?? string.Empty,
        Arquivos = ata.Arquivos.Select(ToArquivoDto).ToList(),
        CreatedAt = ata.CreatedAt,
        UpdatedAt = ata.UpdatedAt,
    };

    private static AtaArquivoDto ToArquivoDto(AtaArquivo arquivo) => new()
    {
        Id = arquivo.Id,
        Nome = arquivo.Nome,
        ContentType = arquivo.ContentType,
        TamanhoBytes = arquivo.TamanhoBytes,
        CreatedAt = arquivo.CreatedAt,
        DownloadUrl = $"/api/atas/{arquivo.AtaId}/arquivos/{arquivo.Id}/download",
    };
}
