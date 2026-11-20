namespace InsumoPro.Application.Features.Onboarding.DTOs;

public class SubmitOnboardingRequest
{
    public Dictionary<string, string> Answers { get; set; } = new();
}