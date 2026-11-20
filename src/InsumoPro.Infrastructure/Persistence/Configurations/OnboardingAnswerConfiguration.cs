using InsumoPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsumoPro.Infrastructure.Persistence.Configurations;

public class OnboardingAnswerConfiguration : IEntityTypeConfiguration<OnboardingAnswer>
{
    public void Configure(EntityTypeBuilder<OnboardingAnswer> builder)
    {
        builder.ToTable("onboarding_answers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuestionKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AnswerValue).IsRequired();
    }
}