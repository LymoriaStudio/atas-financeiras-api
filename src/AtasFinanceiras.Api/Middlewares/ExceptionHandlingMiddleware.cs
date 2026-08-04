using System.Net;
using System.Text.Json;
using AtasFinanceiras.Application.Common.Exceptions;

namespace AtasFinanceiras.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Recurso não encontrado"),
            ValidationException => (HttpStatusCode.BadRequest, "Requisição inválida"),
            ConflictException => (HttpStatusCode.Conflict, "Conflito"),
            UnauthorizedAppException => (HttpStatusCode.Unauthorized, "Não autorizado"),
            _ => (HttpStatusCode.InternalServerError, "Erro interno"),
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Erro não tratado ao processar {Path}", context.Request.Path);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            title,
            status = (int)statusCode,
            detail = exception.Message,
            traceId = context.TraceIdentifier,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
