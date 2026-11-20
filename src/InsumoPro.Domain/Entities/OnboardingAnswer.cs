using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;

namespace InsumoPro.Domain.Entities;

public class OnboardingAnswer : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string QuestionKey { get; set; } = null!;
    public string AnswerValue { get; set; } = null!;

    public Company Company { get; set; } = null!;
}