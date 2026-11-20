using InsumoPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsumoPro.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.LegalName).HasColumnName("legal_name").HasMaxLength(200);
        builder.Property(x => x.Rfc).HasColumnName("rfc").HasMaxLength(20);
        builder.Property(x => x.Country).HasColumnName("country").HasMaxLength(50).HasDefaultValue("MX");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).HasDefaultValue("MXN");
        builder.Property(x => x.LogoUrl).HasColumnName("logo_url");
        builder.Property(x => x.BusinessProfileId).HasColumnName("business_profile_id");
        builder.Property(x => x.PlanId).HasColumnName("plan_id");
        builder.Property(x => x.RecommendedPlanId).HasColumnName("recommended_plan_id");
        builder.Property(x => x.OnboardingCompleted).HasColumnName("onboarding_completed");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(x => x.BusinessProfile)
            .WithMany()
            .HasForeignKey(x => x.BusinessProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RecommendedPlan)
            .WithMany()
            .HasForeignKey(x => x.RecommendedPlanId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}