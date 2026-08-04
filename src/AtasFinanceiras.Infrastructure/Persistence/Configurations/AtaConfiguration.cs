using AtasFinanceiras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtasFinanceiras.Infrastructure.Persistence.Configurations;

public class AtaConfiguration : IEntityTypeConfiguration<Ata>
{
    public void Configure(EntityTypeBuilder<Ata> builder)
    {
        builder.ToTable("atas");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Numero).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Titulo).HasMaxLength(300).IsRequired();
        builder.Property(a => a.Tipo).HasMaxLength(100);
        builder.Property(a => a.Descricao).HasMaxLength(2000);
        builder.Property(a => a.Local).HasMaxLength(200);
        builder.Property(a => a.Presidente).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Secretario).HasMaxLength(200);
        builder.Property(a => a.Participantes).HasMaxLength(2000);

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(a => a.Categoria)
            .WithMany(c => c.Atas)
            .HasForeignKey(a => a.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Arquivos)
            .WithOne(ar => ar.Ata)
            .HasForeignKey(ar => ar.AtaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.DeletedAt);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Data);
    }
}
