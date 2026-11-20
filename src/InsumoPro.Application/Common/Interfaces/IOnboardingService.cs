using InsumoPro.Application.Features.Onboarding.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface IOnboardingService
{
    List<OnboardingQuestionDto> GetQuestions();
    Task<OnboardingResultDto> SubmitAsync(Guid companyId, SubmitOnboardingRequest request);
}