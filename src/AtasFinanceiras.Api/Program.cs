using System.Text;
using AtasFinanceiras.Api.Extensions;
using AtasFinanceiras.Api.Middlewares;
using AtasFinanceiras.Application;
using AtasFinanceiras.Application.Interfaces.Repositories;
using AtasFinanceiras.Application.Interfaces.Services;
using AtasFinanceiras.Domain.Entities;
using AtasFinanceiras.Domain.Enums;
using AtasFinanceiras.Infrastructure;
using AtasFinanceiras.Infrastructure.Auth;
using AtasFinanceiras.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AtasFinanceiras.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Atas Financeiras API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe apenas o token JWT (sem o prefixo 'Bearer ').",
            });

            // Só marca cadeado nas rotas que realmente exigem [Authorize] — rotas públicas
            // (ex: listar/baixar atas) ficam sem cadeado, refletindo o comportamento real.
            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Seção 'Jwt' não configurada.");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            };
        });

        builder.Services.AddAuthorization();

        var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Default", policy =>
            {
                policy.WithOrigins(corsOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Alinhado ao limite de 15 MB do front (com folga) para upload de PDFs/DOCX/XLSX das atas.
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 20 * 1024 * 1024;
        });

        var app = builder.Build();

        await ApplyMigrationsAndSeedAdminAsync(app);

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Swagger fica exposto também fora de Development de propósito: este ambiente
        // ainda é de teste (Railway), e é assim que o cliente valida os endpoints.
        // Antes de apontar dados reais de produção aqui, vale restringir isso.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseCors("Default");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }

    // Aplica migrations pendentes e cria o primeiro usuário Admin se nenhum existir ainda —
    // sem isso, um banco novo fica sem porta de entrada (só Admin pode criar usuários).
    // Só age se Seed:AdminEmail/AdminPassword estiverem configurados; nunca sobrescreve um admin existente.
    // Falha aqui não derruba a API (útil em dev, quando o banco pode não estar disponível ainda).
    private static async Task ApplyMigrationsAndSeedAdminAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();

            var adminEmail = app.Configuration["Seed:AdminEmail"];
            var adminPassword = app.Configuration["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogInformation("Seed:AdminEmail/AdminPassword não configurados — nenhum usuário admin será criado automaticamente.");
                return;
            }

            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            if (await uow.Usuarios.GetByEmailAsync(adminEmail) is not null)
                return;

            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await uow.Usuarios.AddAsync(new Usuario
            {
                FullName = app.Configuration["Seed:AdminFullName"] ?? "Administrador",
                Email = adminEmail,
                PasswordHash = hasher.Hash(adminPassword),
                Role = UserRole.Admin,
                IsActive = true,
            });
            await uow.SaveChangesAsync();

            logger.LogInformation("Usuário admin inicial criado: {Email}", adminEmail);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível aplicar migrations/seed automático na inicialização.");
        }
    }
}
