using Postech.Notifications.Api.Application.Services;

namespace Postech.Notifications.Api.Infrastructure.Email;

public class MockEmailService : IEmailService
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<MockEmailService>();

    public Task SendWelcomeEmailAsync(string toEmail, string userName, Guid userId)
    {
        _logger.Information(
            "[MockEmail] Welcome | To: {To} | Name: {Name} | UserId: {UserId}",
            toEmail,
            userName,
            userId);
        return Task.CompletedTask;
    }

    public Task SendPaymentApprovedEmailAsync(
        string toEmail,
        string userName,
        Guid orderId,
        Guid gameId,
        decimal price)
    {
        _logger.Information(
            "[MockEmail] Payment approved | To: {To} | OrderId: {OrderId} | GameId: {GameId} | Price: {Price}",
            toEmail,
            orderId,
            gameId,
            price);
        return Task.CompletedTask;
    }

    public Task SendPaymentRejectedEmailAsync(
        string toEmail,
        string userName,
        Guid orderId,
        Guid gameId,
        decimal price)
    {
        _logger.Information(
            "[MockEmail] Payment rejected | To: {To} | OrderId: {OrderId} | GameId: {GameId} | Price: {Price}",
            toEmail,
            orderId,
            gameId,
            price);
        return Task.CompletedTask;
    }
}
