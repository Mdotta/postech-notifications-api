using Postech.Notifications.Api.Application.Services;
using Postech.Notifications.Api.Infrastructure.Email;

namespace Postech.Notifications.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpClient();

        // Integração real (Brevo) — descomente quando for usar envio de e-mail de verdade.
        // services.AddScoped<IEmailService, BrevoEmailProvider>();

        services.AddScoped<IEmailService, MockEmailService>();

        return services;
    }
}
