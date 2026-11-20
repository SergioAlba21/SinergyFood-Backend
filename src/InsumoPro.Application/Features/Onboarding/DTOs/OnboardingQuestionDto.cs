namespace InsumoPro.Application.Features.Onboarding.DTOs;

public class OnboardingQuestionDto
{
    public string Key { get; set; } = null!;
    public string Question { get; set; } = null!;
    public string Type { get; set; } = "single"; // single, multiple
    public List<OnboardingOptionDto> Options { get; set; } = new();
}

public class OnboardingOptionDto
{
    public string Value { get; set; } = null!;
    public string Label { get; set; } = null!;
}