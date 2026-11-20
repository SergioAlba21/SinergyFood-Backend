using InsumoPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsumoPro.Infrastructure.Persistence.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PriceMonthly).HasPrecision(10, 2);
        builder.Property(x => x.PriceYearly).HasPrecision(10, 2);
        builder.Property(x => x.Features).HasColumnType("jsonb");

        builder.HasIndex(x => x.Code).IsUnique();
    }
}