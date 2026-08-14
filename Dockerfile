# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AtasFinanceiras.sln", "."]
COPY ["src/AtasFinanceiras.Api/AtasFinanceiras.Api.csproj", "src/AtasFinanceiras.Api/"]
COPY ["src/AtasFinanceiras.Application/AtasFinanceiras.Application.csproj", "src/AtasFinanceiras.Application/"]
COPY ["src/AtasFinanceiras.Domain/AtasFinanceiras.Domain.csproj", "src/AtasFinanceiras.Domain/"]
COPY ["src/AtasFinanceiras.Infrastructure/AtasFinanceiras.Infrastructure.csproj", "src/AtasFinanceiras.Infrastructure/"]
COPY ["src/AtasFinanceiras.Migrations.Postgres/AtasFinanceiras.Migrations.Postgres.csproj", "src/AtasFinanceiras.Migrations.Postgres/"]
COPY ["src/AtasFinanceiras.Migrations.SqlServer/AtasFinanceiras.Migrations.SqlServer.csproj", "src/AtasFinanceiras.Migrations.SqlServer/"]

RUN dotnet restore "src/AtasFinanceiras.Api/AtasFinanceiras.Api.csproj"

COPY . .
WORKDIR /src/src/AtasFinanceiras.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN addgroup --system appgroup \
    && adduser --system --ingroup appgroup appuser \
    && mkdir -p /app/uploads \
    && chown -R appuser:appgroup /app

COPY --from=build /app/publish .
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Fica como root aqui de propósito — o entrypoint precisa de privilégio pra corrigir
# a posse do volume montado em runtime antes de trocar pro usuário sem privilégios
# (appuser) pra rodar a aplicação de fato. Ver entrypoint.sh.
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
