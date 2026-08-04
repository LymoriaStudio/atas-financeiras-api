namespace AtasFinanceiras.Infrastructure.FileStorage;

// Vinculado à seção "FileStorage" do appsettings/variáveis de ambiente.
// BasePath aponta pra um volume montado no Railway; no servidor do cliente
// pode virar uma pasta de rede sem mudar nenhuma linha de código consumidor.
public class LocalFileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string BasePath { get; set; } = "uploads";
}
