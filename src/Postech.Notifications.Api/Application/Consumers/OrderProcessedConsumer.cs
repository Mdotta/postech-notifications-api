using MassTransit;
using Postech.Notifications.Api.Application.Services;
using Postech.Shared.Contracts.Events;

namespace Postech.Notifications.Api.Application.Consumers;

public class OrderProcessedConsumer : IConsumer<OrderProcessedEvent>
{
    private readonly IEmailService _emailService;
    private readonly Serilog.ILogger _logger;

    public OrderProcessedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
        _logger = Serilog.Log.ForContext<OrderProcessedConsumer>();
    }

    public async Task Consume(ConsumeContext<OrderProcessedEvent> context)
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
                    "Consumindo PaymentProcessedEvent | OrderId: {OrderId} | UserId: {UserId} | GameId: {GameId} | Successful: {Status} | FailureReason: {FailureReason}",
                    message.OrderId,
                    message.UserId,
                    message.GameId,
                    message.IsSuccessful,
                    message.FailureReason);

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

                if (message.IsSuccessful)
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
                else
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
