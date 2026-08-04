namespace AtasFinanceiras.Application.DTOs.Auth;

public class ChangePasswordRequest
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}
