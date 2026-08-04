using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AtasFinanceiras.Api.Extensions;

// Faz o cadeado do Swagger refletir a realidade: só aparece em rotas que de fato
// exigem [Authorize] e não têm [AllowAnonymous] sobrepondo. Sem isso, o Swagger mostra
// cadeado em tudo (efeito colateral do AddSecurityRequirement global), o que não bate
// com o comportamento real da API — rotas públicas (ex: listar/baixar atas) não exigem token.
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
        if (hasAllowAnonymous)
            return;

        var hasAuthorize = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ?? false);

        if (!hasAuthorize)
            return;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                },
            },
        };
    }
}
