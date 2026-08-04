using AtasFinanceiras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtasFinanceiras.Infrastructure.Persistence.Configurations;

public class AtaArquivoConfiguration : IEntityTypeConfiguration<AtaArquivo>
{
    public void Configure(EntityTypeBuilder<AtaArquivo> builder)
    {
        builder.ToTable("ata_arquivos");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nome).HasMaxLength(300).IsRequired();
        builder.Property(a => a.CaminhoArmazenamento).HasMaxLength(500).IsRequired();
        builder.Property(a => a.ContentType).HasMaxLength(150).IsRequired();
    }
}
