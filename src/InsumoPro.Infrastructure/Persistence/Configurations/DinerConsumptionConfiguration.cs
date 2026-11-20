using InsumoPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsumoPro.Infrastructure.Persistence.Configurations;

public class DinerConsumptionConfiguration : IEntityTypeConfiguration<DinerConsumption>
{
    public void Configure(EntityTypeBuilder<DinerConsumption> builder)
    {
        builder.ToTable("diner_consumptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyId).HasColumnName("company_id");
        builder.Property(x => x.DinerId).HasColumnName("diner_id");
        builder.Property(x => x.MealTypeId).HasColumnName("meal_type_id");
        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.ValidatedBy).HasColumnName("validated_by");

        builder.Property(x => x.Quantity)
            .HasPrecision(12, 3)
            .HasDefaultValue(1m);

        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.Property(x => x.Folio).HasMaxLength(50);
        builder.Property(x => x.Printed).HasDefaultValue(false);
        builder.Property(x => x.PrintedAt);

        builder.Property(x => x.IsProvisional).HasDefaultValue(false);
        builder.Property(x => x.GuestName).HasMaxLength(150);
        builder.Property(x => x.GuestNote).HasMaxLength(300);

        builder.HasIndex(x => new { x.CompanyId, x.DinerId, x.ConsumedAt });
        builder.HasIndex(x => new { x.CompanyId, x.Printed });
        builder.HasIndex(x => new { x.CompanyId, x.IsProvisional });

        builder.HasIndex(x => new { x.CompanyId, x.Folio })
            .IsUnique()
            .HasFilter("folio IS NOT NULL");

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Diner opcional (cupón provisional)
        builder.HasOne(x => x.Diner)
            .WithMany(d => d.Consumptions)
            .HasForeignKey(x => x.DinerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(x => x.MealType)
            .WithMany()
            .HasForeignKey(x => x.MealTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(x => x.Validator)
            .WithMany()
            .HasForeignKey(x => x.ValidatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}