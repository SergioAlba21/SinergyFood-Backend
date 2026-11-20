using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace InsumoPro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Mapster (si lo vas a usar)
        // services.AddMapster();

        // Aquí después registraremos MediatR si lo usamos:
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}