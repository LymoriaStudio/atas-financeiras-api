using AtasFinanceiras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtasFinanceiras.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();

        // Enum salvo como string: legível no banco e idêntico em Postgres/SQL Server
        // (evita depender de como cada provider trata o tipo integer subjacente).
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(u => u.JobTitle).HasMaxLength(150);
        builder.Property(u => u.Department).HasMaxLength(150);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.RefreshToken).HasMaxLength(200);
    }
}
