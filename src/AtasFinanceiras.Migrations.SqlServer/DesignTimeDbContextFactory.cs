using AtasFinanceiras.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtasFinanceiras.Migrations.SqlServer;

// Usado só pela CLI (`dotnet ef migrations add` / `database update`) para saber
// como construir o AppDbContext neste projeto. A connection string aqui é só um
// placeholder de design-time — em runtime real, quem decide é
// AtasFinanceiras.Infrastructure.DependencyInjection, a partir da configuração da Api.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=AtasFinanceirasDesignTime;Trusted_Connection=True;TrustServerCertificate=True;",
            sql => sql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}
