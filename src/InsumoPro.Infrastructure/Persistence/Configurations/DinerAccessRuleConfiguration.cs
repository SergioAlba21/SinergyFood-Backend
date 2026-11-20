using InsumoPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsumoPro.Infrastructure.Persistence.Configurations;

public class DinerAccessRuleConfiguration : IEntityTypeConfiguration<DinerAccessRule>
{
    public void Configure(EntityTypeBuilder<DinerAccessRule> builder)
    {
        builder.ToTable("diner_access_rules");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.DinerId, x.MealTypeId }).IsUnique();

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Diner)
            .WithMany(d => d.AccessRules)
            .HasForeignKey(x => x.DinerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MealType)
            .WithMany()
            .HasForeignKey(x => x.MealTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}