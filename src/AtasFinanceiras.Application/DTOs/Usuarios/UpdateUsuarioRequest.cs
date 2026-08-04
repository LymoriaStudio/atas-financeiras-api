namespace AtasFinanceiras.Application.DTOs.Usuarios;

public class UpdateUsuarioRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "viewer";
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
