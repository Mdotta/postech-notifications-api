using Postech.Notifications.Api.Extensions;
using Postech.Notifications.Api.Infrastructure.MassTransit;
using Prometheus;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region [Logging Configuration]

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Notifications.Api")
    .CreateLogger();

builder.Host.UseSerilog((context, services, options) =>
{
    options
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});

#endregion

builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

builder.Services.AddMassTransitServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddMongoDb(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");
app.MapMetrics("/metrics").AllowAnonymous();
app.UseHttpMetrics(options => options.AddCustomLabel("service", _ => "notifications-api"));

app.Run();
