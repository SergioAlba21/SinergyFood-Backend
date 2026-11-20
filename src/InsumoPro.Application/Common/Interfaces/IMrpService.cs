using InsumoPro.Application.Features.Mrp.DTOs;

namespace InsumoPro.Application.Common.Interfaces;

public interface IMrpService
{
    Task<List<MrpSuggestionDto>> GenerateSuggestionsAsync(Guid companyId);
    Task<List<MrpSuggestionDto>> GetSuggestionsAsync(Guid companyId, string? status = "pending");
    Task<object> ConvertToPurchaseOrderAsync(Guid companyId, Guid userId, Guid suggestionId, ConvertSuggestionRequest request);
    Task<bool> DismissAsync(Guid companyId, Guid suggestionId);
}