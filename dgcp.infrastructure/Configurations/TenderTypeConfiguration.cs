using dgcp.domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dgcp.infrastructure.Configurations
{
    internal class TenderTypeConfiguration : IEntityTypeConfiguration<Tender>
    {
        public void Configure(EntityTypeBuilder<Tender> builder)
        {
            builder.Property(e => e.ReleaseId).IsRequired().HasMaxLength(100);
            builder.Property(e => e.ReleaseOcid).IsRequired().HasMaxLength(100);
            builder.Property(e => e.TenderId).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Publisher).HasMaxLength(200);
            builder.Property(e => e.Currency).HasMaxLength(10);
            builder.Property(e => e.Status).HasMaxLength(30);
            builder.Property(e => e.ProcuringEntity).HasMaxLength(500);
        }
    }
}