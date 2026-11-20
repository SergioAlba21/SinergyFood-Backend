using InsumoPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsumoPro.Infrastructure.Persistence.Configurations;

public class OnboardingRecommendationConfiguration : IEntityTypeConfiguration<OnboardingRecommendation>
{
    public void Configure(EntityTypeBuilder<OnboardingRecommendation> builder)
    {
        builder.ToTable("onboarding_recommendations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500);
    }
}