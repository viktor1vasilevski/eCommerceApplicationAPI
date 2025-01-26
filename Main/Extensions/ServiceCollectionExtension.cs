using Data.Repositories;
using EntityModels.Interfaces;
using Main.Interfaces;
using Main.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddIoCService(this IServiceCollection services)
    {
        services.AddScoped(typeof(IUnitOfWork<>), typeof(SqlUnitOfWork<>));

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
