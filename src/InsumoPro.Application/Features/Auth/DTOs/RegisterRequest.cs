namespace InsumoPro.Application.Features.Auth.DTOs;

public class RegisterRequest
{
    public string CompanyName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Phone { get; set; }
}