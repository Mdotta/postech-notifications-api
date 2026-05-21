using MassTransit;
using Postech.Notifications.Api.Application.Services;
using Postech.Notifications.Api.Infrastructure.Metrics;
using Postech.Notifications.Api.Infrastructure.MongoDB.Documents;
using Postech.Notifications.Api.Infrastructure.MongoDB.Repositories;
using Postech.Shared.Contracts.Events;

namespace Postech.Notifications.Api.Application.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IEventLogRepository? _eventLogRepository;
    private readonly Serilog.ILogger _logger;

    public UserCreatedConsumer(IEmailService emailService, IEventLogRepository? eventLogRepository = null)
    {
        _emailService = emailService;
        _eventLogRepository = eventLogRepository;
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

                await _emailService.SendWelcomeEmailAsync(message.Email, message.Name, message.UserId);

                NotificationsMetrics.EmailsSent.WithLabels("welcome").Inc();

                _logger.Information(
                    "Email de boas-vindas enviado com sucesso | UserId: {UserId} | Email: {Email}",
                    message.UserId,
                    message.Email);

                await SaveEventLogAsync(new EventLogDocument
                {
                    EventType = "UserCreated",
                    Status = "Success",
                    CorrelationId = context.CorrelationId,
                    Payload = new Dictionary<string, string>
                    {
                        ["userId"] = message.UserId.ToString(),
                        ["email"] = message.Email,
                        ["name"] = message.Name
                    }
                }, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Erro ao processar UserCreatedEvent | UserId: {UserId} | Email: {Email}",
                    message.UserId,
                    message.Email);

                await SaveEventLogAsync(new EventLogDocument
                {
                    EventType = "UserCreated",
                    Status = "Failed",
                    CorrelationId = context.CorrelationId,
                    Payload = new Dictionary<string, string>
                    {
                        ["userId"] = message.UserId.ToString(),
                        ["email"] = message.Email,
                        ["name"] = message.Name
                    },
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name
                }, context.CancellationToken);

                throw;
            }
        }
    }

    private async Task SaveEventLogAsync(EventLogDocument document, CancellationToken cancellationToken)
    {
        if (_eventLogRepository is null) return;
        try
        {
            await _eventLogRepository.InsertAsync(document, cancellationToken);
            NotificationsMetrics.EventLogsPersisted.WithLabels("success").Inc();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Falha ao salvar event log no MongoDB. Processamento do evento não foi afetado.");
            NotificationsMetrics.EventLogsPersisted.WithLabels("failure").Inc();
        }
    }
}
