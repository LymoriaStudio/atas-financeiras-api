namespace AtasFinanceiras.Application.Common.Exceptions;

// Nome diferente de UnauthorizedAccessException (BCL) para evitar colisão de using
// e deixar explícito que é um erro de regra de negócio (credenciais inválidas etc.),
// não uma falha de permissão de sistema operacional.
public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message) : base(message) { }
}
