using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Onboarding.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsumoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;

    public OnboardingController(IOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    [HttpGet("questions")]
    public ActionResult<List<OnboardingQuestionDto>> GetQuestions()
    {
        return Ok(_onboardingService.GetQuestions());
    }

    [HttpPost("submit")]
    public async Task<ActionResult<OnboardingResultDto>> Submit([FromBody] SubmitOnboardingRequest request)
    {
        var companyIdClaim = User.FindFirstValue("company_id");
        if (!Guid.TryParse(companyIdClaim, out var companyId))
            return Unauthorized();

        try
        {
            var result = await _onboardingService.SubmitAsync(companyId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}