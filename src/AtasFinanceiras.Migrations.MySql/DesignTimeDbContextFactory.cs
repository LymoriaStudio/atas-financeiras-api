using AtasFinanceiras.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtasFinanceiras.Migrations.MySql;

// Usado só pela CLI (`dotnet ef migrations add` / `database update`) para saber
// como construir o AppDbContext neste projeto. A connection string aqui é só um
// placeholder de design-time — em runtime real, quem decide é
// AtasFinanceiras.Infrastructure.DependencyInjection, a partir da configuração da Api.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = "Server=localhost;Port=3306;Database=atas_financeiras_designtime;Uid=root;Pwd=root;";

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.Create(new Version(8, 0, 0), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql),
            mysql => mysql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}
