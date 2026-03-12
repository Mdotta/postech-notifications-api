using Postech.Notifications.Api.Application.Services;
using Postech.Notifications.Api.Infrastructure.Email;

namespace Postech.Notifications.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IEmailService, BrevoEmailProvider>();
        return services;
    }
}
