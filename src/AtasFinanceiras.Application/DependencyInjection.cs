using AtasFinanceiras.Application.Interfaces.Services;
using AtasFinanceiras.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AtasFinanceiras.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IAtaService, AtaService>();
        services.AddScoped<IAtividadeService, AtividadeService>();

        return services;
    }
}
