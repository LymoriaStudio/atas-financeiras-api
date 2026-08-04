using AtasFinanceiras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtasFinanceiras.Infrastructure.Persistence.Configurations;

public class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.ToTable("notificacoes");
        builder.HasKey(n => n.Id);

        builder.HasOne(n => n.Atividade)
            .WithMany(a => a.Notificacoes)
            .HasForeignKey(n => n.AtividadeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Usuario)
            .WithMany(u => u.Notificacoes)
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.AtividadeId, n.UsuarioId }).IsUnique();
    }
}
