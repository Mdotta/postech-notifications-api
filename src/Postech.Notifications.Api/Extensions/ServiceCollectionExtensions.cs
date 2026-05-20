using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using MongoDB.Driver;
using Postech.Notifications.Api.Application.Services;
using Postech.Notifications.Api.Infrastructure.DynamoDB;
using Postech.Notifications.Api.Infrastructure.DynamoDB.Repositories;
using Postech.Notifications.Api.Infrastructure.Email;
using Postech.Notifications.Api.Infrastructure.MongoDB;
using Postech.Notifications.Api.Infrastructure.MongoDB.Repositories;

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

    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        var dynamoSettings = configuration.GetSection("DynamoDB").Get<DynamoDbSettings>();
        if (dynamoSettings is not null && dynamoSettings.UseDynamoDB && !string.IsNullOrWhiteSpace(dynamoSettings.TableName))
        {
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddScoped<IEventLogRepository>(sp =>
            {
                var dynamoDb = sp.GetRequiredService<IAmazonDynamoDB>();
                return new EventLogDynamoRepository(dynamoDb, dynamoSettings.TableName);
            });
            return services;
        }

        var mongoSettings = configuration.GetSection("MongoDB").Get<MongoDbSettings>();
        if (mongoSettings is null || string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
            return services;

        var mongoClient = new MongoClient(mongoSettings.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoSettings.DatabaseName);
        services.AddSingleton(mongoDatabase);
        services.AddScoped<IEventLogRepository, EventLogRepository>();

        return services;
    }
}
