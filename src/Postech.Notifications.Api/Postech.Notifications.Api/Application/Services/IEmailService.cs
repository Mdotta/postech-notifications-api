namespace Postech.Notifications.Api.Application.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string userName, Guid userId);
    Task SendPaymentApprovedEmailAsync(string toEmail, string userName, Guid orderId, Guid gameId, decimal price);
    Task SendPaymentRejectedEmailAsync(string toEmail, string userName, Guid orderId, Guid gameId, decimal price);
}
