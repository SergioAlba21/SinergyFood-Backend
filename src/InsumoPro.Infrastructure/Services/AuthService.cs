using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Auth.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InsumoPro.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validar si el email ya existe
        if (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email))
            throw new Exception("El correo ya está registrado.");

        // Crear la empresa
        var company = new Company
        {
            Name = request.CompanyName,
            OnboardingCompleted = false
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        // Crear usuario administrador
        var user = new User
        {
            CompanyId = company.Id,
            Name = request.Name,
            Email = request.Email.ToLower().Trim(),
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "admin",
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return GenerateToken(user, company);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Correo o contraseña incorrectos.");

        if (!user.IsActive)
            throw new Exception("El usuario está desactivado.");

        user.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return GenerateToken(user, user.Company);
    }

    private AuthResponse GenerateToken(User user, Company company)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("company_id", company.Id.ToString())
        };

        var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");
        var expiration = DateTime.Now.AddMinutes(expireMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            UserId = user.Id,
            CompanyId = company.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            OnboardingCompleted = company.OnboardingCompleted
        };
    }
}