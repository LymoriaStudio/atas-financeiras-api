using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Application.Interfaces.Services;
using AtasFinanceiras.Infrastructure.Auth;
using AtasFinanceiras.Infrastructure.FileStorage;
using AtasFinanceiras.Infrastructure.Persistence;
using AtasFinanceiras.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtasFinanceiras.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Postgres";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (provider.Trim().ToLowerInvariant())
            {
                case "sqlserver":
                    options.UseSqlServer(connectionString, sql => sql
                        .MigrationsAssembly("AtasFinanceiras.Migrations.SqlServer"));
                    break;
                case "mysql":
                    // Versão fixa (não AutoDetect) de propósito: AutoDetect conectaria no banco
                    // já na inicialização da aplicação, quebrando a resiliência que os outros
                    // providers têm (a app sobe mesmo com o banco fora do ar, só falha na query).
                    options.UseMySql(
                        connectionString,
                        ServerVersion.Create(new Version(8, 0, 0), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql),
                        mysql => mysql.MigrationsAssembly("AtasFinanceiras.Migrations.MySql"));
                    break;
                case "postgres":
                default:
                    options.UseNpgsql(connectionString, npg => npg
                        .MigrationsAssembly("AtasFinanceiras.Migrations.Postgres"));
                    break;
            }
        });

        services.AddScoped<IAtaRepository, AtaRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IAtividadeRepository, AtividadeRepository>();
        services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<LocalFileStorageOptions>(configuration.GetSection(LocalFileStorageOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
