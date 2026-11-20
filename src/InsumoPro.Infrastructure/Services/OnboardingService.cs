using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Onboarding.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Services;

public class OnboardingService : IOnboardingService
{
    private readonly ApplicationDbContext _context;

    public OnboardingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<OnboardingQuestionDto> GetQuestions()
    {
        return new List<OnboardingQuestionDto>
        {
            new()
            {
                Key = "business_type",
                Question = "¿Qué tipo de negocio tienes?",
                Options = new List<OnboardingOptionDto>
                {
                    new() { Value = "gomitas", Label = "Productor de gomitas / dulces" },
                    new() { Value = "snack_center", Label = "Snack Center / Antojitos" },
                    new() { Value = "comedor_industrial", Label = "Comedor Industrial" },
                    new() { Value = "panaderia", Label = "Panadería / Repostería" },
                    new() { Value = "mixto", Label = "Negocio mixto (compro y fabrico)" },
                    new() { Value = "solo_reventa", Label = "Solo reventa" }
                }
            },
            new()
            {
                Key = "fabricates",
                Question = "¿Fabricas o preparas productos / platillos?",
                Options = new List<OnboardingOptionDto>
                {
                    new() { Value = "mostly", Label = "Sí, la mayoría" },
                    new() { Value = "some", Label = "Sí, algunos" },
                    new() { Value = "no", Label = "No, solo compro y vendo" }
                }
            },
            new()
            {
                Key = "uses_recipes",
                Question = "¿Usas recetas o fórmulas definidas?",
                Options = new List<OnboardingOptionDto>
                {
                    new() { Value = "yes", Label = "Sí, tengo recetas definidas" },
                    new() { Value = "sometimes", Label = "A veces" },
                    new() { Value = "no", Label = "No" }
                }
            },
            new()
            {
                Key = "stock_problems",
                Question = "¿Con qué frecuencia se te acaban los insumos o compras de más?",
                Options = new List<OnboardingOptionDto>
                {
                    new() { Value = "often", Label = "Muy seguido" },
                    new() { Value = "sometimes", Label = "A veces" },
                    new() { Value = "rarely", Label = "Casi nunca" }
                }
            },
            new()
            {
                Key = "needs_diner_control",
                Question = "¿Necesitas controlar el acceso de comensales / empleados?",
                Options = new List<OnboardingOptionDto>
                {
                    new() { Value = "yes", Label = "Sí, es importante" },
                    new() { Value = "no", Label = "No" }
                }
            },
            new()
            {
                Key = "multiple_warehouses",
                Question = "¿Tienes más de un almacén o sucursal?",
                Options = new List<OnboardingOptionDto>
                {
                    new() { Value = "yes", Label = "Sí" },
                    new() { Value = "no", Label = "No" }
                }
            }
        };
    }

    public async Task<OnboardingResultDto> SubmitAsync(Guid companyId, SubmitOnboardingRequest request)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == companyId)
            ?? throw new Exception("Empresa no encontrada");

        // Guardar respuestas
        foreach (var answer in request.Answers)
        {
            _context.Set<OnboardingAnswer>().Add(new OnboardingAnswer
            {
                CompanyId = companyId,
                QuestionKey = answer.Key,
                AnswerValue = answer.Value
            });
        }

        // Lógica de recomendación
        var businessType = request.Answers.GetValueOrDefault("business_type", "mixto");
        var fabricates = request.Answers.GetValueOrDefault("fabricates", "some");
        var usesRecipes = request.Answers.GetValueOrDefault("uses_recipes", "sometimes");
        var stockProblems = request.Answers.GetValueOrDefault("stock_problems", "sometimes");
        var needsDinerControl = request.Answers.GetValueOrDefault("needs_diner_control", "no");
        var multipleWarehouses = request.Answers.GetValueOrDefault("multiple_warehouses", "no");

        string recommendedPlanCode;
        string reason;
        string profileCode = businessType;

        // Reglas de recomendación
        if (needsDinerControl == "yes" || businessType == "comedor_industrial" || multipleWarehouses == "yes")
        {
            recommendedPlanCode = "complete";
            reason = "Necesitas control de comensales o múltiples almacenes. El Plan Completo es el más adecuado.";
        }
        else if (fabricates == "mostly" || usesRecipes == "yes" || stockProblems == "often")
        {
            recommendedPlanCode = "production";
            reason = "Fabricas productos y/o necesitas control de costos y compras inteligentes.";
        }
        else
        {
            recommendedPlanCode = "basic";
            reason = "Tu operación es principalmente de compra y venta. El Plan Básico es suficiente.";
        }

        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Code == recommendedPlanCode);
        var profile = await _context.BusinessProfiles.FirstOrDefaultAsync(p => p.Code == profileCode);

        company.RecommendedPlanId = plan?.Id;
        company.PlanId = plan?.Id;
        company.BusinessProfileId = profile?.Id;
        company.OnboardingCompleted = true;

        // Guardar recomendación
        _context.Set<OnboardingRecommendation>().Add(new OnboardingRecommendation
        {
            CompanyId = companyId,
            RecommendedPlanId = plan?.Id,
            Reason = reason,
            ScoreBasic = recommendedPlanCode == "basic" ? 100 : 30,
            ScoreProduction = recommendedPlanCode == "production" ? 100 : 40,
            ScoreComplete = recommendedPlanCode == "complete" ? 100 : 20
        });

        await _context.SaveChangesAsync();

        return new OnboardingResultDto
        {
            RecommendedPlanCode = recommendedPlanCode,
            RecommendedPlanName = plan?.Name ?? recommendedPlanCode,
            Reason = reason,
            BusinessProfileCode = profileCode,
            EnabledModules = GetEnabledModules(recommendedPlanCode, needsDinerControl == "yes")
        };
    }

    private List<string> GetEnabledModules(string planCode, bool hasDinerControl)
    {
        var modules = new List<string> { "inventario", "compras", "ventas" };

        if (planCode is "production" or "complete")
        {
            modules.AddRange(new[] { "recetas", "produccion", "mini_mrp" });
        }

        if (planCode == "complete" || hasDinerControl)
        {
            modules.Add("comensales");
        }

        return modules;
    }
}