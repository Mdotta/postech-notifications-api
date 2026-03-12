using MassTransit;
using Postech.Notifications.Api.Application.Services;
using Postech.Notifications.Api.Domain.Events;

namespace Postech.Notifications.Api.Application.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IEmailService _emailService;
    private readonly Serilog.ILogger _logger;

    public PaymentProcessedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
        _logger = Serilog.Log.ForContext<PaymentProcessedConsumer>();
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", context.CorrelationId))
        using (Serilog.Context.LogContext.PushProperty("OrderId", message.OrderId))
        using (Serilog.Context.LogContext.PushProperty("UserId", message.UserId))
        using (Serilog.Context.LogContext.PushProperty("GameId", message.GameId))
        {
            try
            {
                _logger.Information(
                    "Consumindo PaymentProcessedEvent | OrderId: {OrderId} | UserId: {UserId} | GameId: {GameId} | Status: {Status}",
                    message.OrderId,
                    message.UserId,
                    message.GameId,
                    message.Status);

                // TODO: Buscar email do usuário através de uma API de usuários ou banco de dados
                // Por enquanto, usando um email mock baseado no UserId para testes
                // Em produção, isso deve ser substituído por uma chamada real ao serviço de usuários
                var userEmail = $"user_{message.UserId}@example.com";
                var userName = $"Usuário {message.UserId}";

                // TODO: O PaymentProcessedEvent não inclui o Price atualmente
                // Opções: 1) Adicionar Price ao evento no Payments API
                //         2) Buscar o preço através de uma API de pedidos
                // Por enquanto usando um valor padrão para demonstração
                var price = 0m;

                if (message.Status == PaymentStatus.Completed)
                {
                    await _emailService.SendPaymentApprovedEmailAsync(
                        userEmail,
                        userName,
                        message.OrderId,
                        message.GameId,
                        price);

                    _logger.Information(
                        "Email de pagamento aprovado enviado | OrderId: {OrderId} | Email: {Email}",
                        message.OrderId,
                        userEmail);
                }
                else if (message.Status == PaymentStatus.Failed)
                {
                    await _emailService.SendPaymentRejectedEmailAsync(
                        userEmail,
                        userName,
                        message.OrderId,
                        message.GameId,
                        price);

                    _logger.Information(
                        "Email de pagamento rejeitado enviado | OrderId: {OrderId} | Email: {Email}",
                        message.OrderId,
                        userEmail);
                }
                else
                {
                    _logger.Information(
                        "Status de pagamento não requer envio de email | OrderId: {OrderId} | Status: {Status}",
                        message.OrderId,
                        message.Status);
                }

                _logger.Information(
                    "PaymentProcessedEvent processado com sucesso | OrderId: {OrderId}",
                    message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Erro ao processar PaymentProcessedEvent | OrderId: {OrderId}",
                    message.OrderId);
                throw;
            }
        }
    }
}
