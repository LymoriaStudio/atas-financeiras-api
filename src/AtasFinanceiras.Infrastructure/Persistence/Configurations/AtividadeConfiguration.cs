using AtasFinanceiras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtasFinanceiras.Infrastructure.Persistence.Configurations;

public class AtividadeConfiguration : IEntityTypeConfiguration<Atividade>
{
    public void Configure(EntityTypeBuilder<Atividade> builder)
    {
        builder.ToTable("atividades");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Acao).HasMaxLength(300).IsRequired();
        builder.Property(a => a.Documento).HasMaxLength(300);

        builder.HasOne(a => a.Usuario)
            .WithMany(u => u.Atividades)
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.CreatedAt);
    }
}
