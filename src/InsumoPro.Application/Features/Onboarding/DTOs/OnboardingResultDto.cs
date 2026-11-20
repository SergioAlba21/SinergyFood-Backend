namespace InsumoPro.Application.Features.Onboarding.DTOs;

public class OnboardingResultDto
{
    public string RecommendedPlanCode { get; set; } = null!;
    public string RecommendedPlanName { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string BusinessProfileCode { get; set; } = null!;
    public List<string> EnabledModules { get; set; } = new();
}