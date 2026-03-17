using MassTransit;
using Postech.Notifications.Api.Application.Services;
using Postech.Notifications.Api.Domain.Events;

namespace Postech.Notifications.Api.Application.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly Serilog.ILogger _logger;

    public UserCreatedConsumer()
    {
        _logger = Serilog.Log.ForContext<UserCreatedConsumer>();
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", context.CorrelationId))
        using (Serilog.Context.LogContext.PushProperty("UserId", message.UserId))
        using (Serilog.Context.LogContext.PushProperty("Email", message.Email))
        {
            try
            {
                _logger.Information(
                    "Consumindo UserCreatedEvent | UserId: {UserId} | Email: {Email} | Name: {Name}",
                    message.UserId,
                    message.Email,
                    message.Name);
                await Task.Delay(1000);
                _logger.Information(
                    "Email de boas-vindas enviado com sucesso | UserId: {UserId} | Email: {Email}",
                    message.UserId,
                    message.Email);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Erro ao processar UserCreatedEvent | UserId: {UserId} | Email: {Email}",
                    message.UserId,
                    message.Email);
                throw;
            }
        }
    }
}
