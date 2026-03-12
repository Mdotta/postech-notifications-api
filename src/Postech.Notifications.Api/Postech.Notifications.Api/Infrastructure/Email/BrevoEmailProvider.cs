using System.Text;
using System.Text.Json;
using Postech.Notifications.Api.Application.Services;

namespace Postech.Notifications.Api.Infrastructure.Email;

public class BrevoEmailProvider : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Serilog.ILogger _logger;
    private readonly string _apiKey;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private const string BrevoApiUrl = "https://api.brevo.com/v3/smtp/email";

    public BrevoEmailProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = Serilog.Log.ForContext<BrevoEmailProvider>();
        _apiKey = _configuration["Brevo:ApiKey"] ?? throw new InvalidOperationException("Brevo API Key não configurada");
        _senderEmail = _configuration["Brevo:SenderEmail"] ?? "noreply@fiapcloudgames.com";
        _senderName = _configuration["Brevo:SenderName"] ?? "FIAP Cloud Games";
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, Guid userId)
    {
        try
        {
            _logger.Information(
                "Enviando email de boas-vindas | Email: {Email} | UserId: {UserId}",
                toEmail,
                userId);

            var emailRequest = new
            {
                sender = new
                {
                    email = _senderEmail,
                    name = _senderName
                },
                to = new[]
                {
                    new
                    {
                        email = toEmail,
                        name = userName
                    }
                },
                subject = "Bem-vindo à FIAP Cloud Games! 🎮",
                htmlContent = GetWelcomeEmailTemplate(userName, userId)
            };

            var jsonContent = JsonSerializer.Serialize(emailRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://api.brevo.com");
            httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await httpClient.PostAsync("/v3/smtp/email", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.Error(
                    "Erro ao enviar email via Brevo | StatusCode: {StatusCode} | Error: {Error}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException($"Erro ao enviar email: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BrevoEmailResponse>(responseContent);

            _logger.Information(
                "Email de boas-vindas enviado com sucesso | Email: {Email} | UserId: {UserId} | MessageId: {MessageId}",
                toEmail,
                userId,
                result?.MessageId ?? "N/A");
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Erro ao enviar email de boas-vindas | Email: {Email} | UserId: {UserId}",
                toEmail,
                userId);
            throw;
        }
    }

    public async Task SendPaymentApprovedEmailAsync(string toEmail, string userName, Guid orderId, Guid gameId, decimal price)
    {
        try
        {
            _logger.Information(
                "Enviando email de pagamento aprovado | Email: {Email} | OrderId: {OrderId}",
                toEmail,
                orderId);

            var emailRequest = new
            {
                sender = new
                {
                    email = _senderEmail,
                    name = _senderName
                },
                to = new[]
                {
                    new
                    {
                        email = toEmail,
                        name = userName
                    }
                },
                subject = "Pagamento Aprovado - FIAP Cloud Games",
                htmlContent = GetPaymentApprovedTemplate(userName, orderId, gameId, price)
            };

            var jsonContent = JsonSerializer.Serialize(emailRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://api.brevo.com");
            httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await httpClient.PostAsync("/v3/smtp/email", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.Error(
                    "Erro ao enviar email via Brevo | StatusCode: {StatusCode} | Error: {Error}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException($"Erro ao enviar email: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BrevoEmailResponse>(responseContent);

            _logger.Information(
                "Email de pagamento aprovado enviado com sucesso | Email: {Email} | OrderId: {OrderId} | MessageId: {MessageId}",
                toEmail,
                orderId,
                result?.MessageId ?? "N/A");
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Erro ao enviar email de pagamento aprovado | Email: {Email} | OrderId: {OrderId}",
                toEmail,
                orderId);
            throw;
        }
    }

    public async Task SendPaymentRejectedEmailAsync(string toEmail, string userName, Guid orderId, Guid gameId, decimal price)
    {
        try
        {
            _logger.Information(
                "Enviando email de pagamento rejeitado | Email: {Email} | OrderId: {OrderId}",
                toEmail,
                orderId);

            var emailRequest = new
            {
                sender = new
                {
                    email = _senderEmail,
                    name = _senderName
                },
                to = new[]
                {
                    new
                    {
                        email = toEmail,
                        name = userName
                    }
                },
                subject = "Pagamento Rejeitado - FIAP Cloud Games",
                htmlContent = GetPaymentRejectedTemplate(userName, orderId, gameId, price)
            };

            var jsonContent = JsonSerializer.Serialize(emailRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://api.brevo.com");
            httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await httpClient.PostAsync("/v3/smtp/email", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.Error(
                    "Erro ao enviar email via Brevo | StatusCode: {StatusCode} | Error: {Error}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException($"Erro ao enviar email: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BrevoEmailResponse>(responseContent);

            _logger.Information(
                "Email de pagamento rejeitado enviado com sucesso | Email: {Email} | OrderId: {OrderId} | MessageId: {MessageId}",
                toEmail,
                orderId,
                result?.MessageId ?? "N/A");
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Erro ao enviar email de pagamento rejeitado | Email: {Email} | OrderId: {OrderId}",
                toEmail,
                orderId);
            throw;
        }
    }

    private class BrevoEmailResponse
    {
        public string? MessageId { get; set; }
    }

    private string GetWelcomeEmailTemplate(string userName, Guid userId)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            line-height: 1.6; 
            color: #333; 
            margin: 0; 
            padding: 0; 
            background-color: #f4f4f4;
        }}
        .container {{ 
            max-width: 600px; 
            margin: 0 auto; 
            background-color: #ffffff;
        }}
        .header {{ 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white; 
            padding: 40px 20px; 
            text-align: center; 
        }}
        .header h1 {{
            margin: 0;
            font-size: 32px;
            font-weight: bold;
        }}
        .header .emoji {{
            font-size: 48px;
            margin-bottom: 10px;
        }}
        .content {{ 
            padding: 40px 30px; 
        }}
        .welcome-message {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .welcome-message h2 {{
            color: #667eea;
            margin: 0 0 15px 0;
            font-size: 24px;
        }}
        .welcome-message p {{
            color: #666;
            font-size: 16px;
            margin: 10px 0;
        }}
        .features {{
            background-color: #f9f9f9;
            padding: 30px;
            border-radius: 8px;
            margin: 30px 0;
        }}
        .features h3 {{
            color: #333;
            margin-top: 0;
            font-size: 20px;
        }}
        .feature-item {{
            display: flex;
            align-items: center;
            margin: 15px 0;
            padding: 10px;
        }}
        .feature-icon {{
            font-size: 24px;
            margin-right: 15px;
            width: 40px;
            text-align: center;
        }}
        .feature-text {{
            flex: 1;
        }}
        .feature-text strong {{
            color: #667eea;
            display: block;
            margin-bottom: 5px;
        }}
        .cta-button {{
            display: block;
            text-align: center;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 16px 32px;
            text-decoration: none;
            border-radius: 8px;
            font-weight: bold;
            font-size: 16px;
            margin: 30px 0;
            transition: transform 0.2s;
        }}
        .cta-button:hover {{
            transform: translateY(-2px);
        }}
        .info-box {{
            background-color: #e8f4f8;
            border-left: 4px solid #667eea;
            padding: 20px;
            margin: 30px 0;
            border-radius: 4px;
        }}
        .info-box p {{
            margin: 5px 0;
            color: #555;
        }}
        .footer {{ 
            text-align: center; 
            padding: 30px 20px;
            background-color: #f9f9f9;
            color: #666; 
            font-size: 12px; 
        }}
        .footer p {{
            margin: 5px 0;
        }}
        .social-links {{
            margin: 20px 0;
        }}
        .social-links a {{
            color: #667eea;
            text-decoration: none;
            margin: 0 10px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='emoji'>🎮</div>
            <h1>Bem-vindo à FIAP Cloud Games!</h1>
        </div>
        <div class='content'>
            <div class='welcome-message'>
                <h2>Olá, {userName}!</h2>
                <p>Estamos muito felizes em tê-lo conosco! 🎉</p>
            </div>
            
            <p>Sua conta foi criada com sucesso e você já pode começar a explorar nosso catálogo de jogos incríveis.</p>
            
            <div class='features'>
                <h3>O que você pode fazer agora:</h3>
                
                <div class='feature-item'>
                    <div class='feature-icon'>🎯</div>
                    <div class='feature-text'>
                        <strong>Explorar o Catálogo</strong>
                        Descubra centenas de jogos disponíveis para você
                    </div>
                </div>
                
                <div class='feature-item'>
                    <div class='feature-icon'>🛒</div>
                    <div class='feature-text'>
                        <strong>Comprar Jogos</strong>
                        Processo de compra rápido e seguro
                    </div>
                </div>
                
                <div class='feature-item'>
                    <div class='feature-icon'>💳</div>
                    <div class='feature-text'>
                        <strong>Múltiplas Formas de Pagamento</strong>
                        Aceitamos diversos métodos de pagamento
                    </div>
                </div>
                
                <div class='feature-item'>
                    <div class='feature-icon'>📧</div>
                    <div class='feature-text'>
                        <strong>Notificações em Tempo Real</strong>
                        Receba atualizações sobre seus pedidos por email
                    </div>
                </div>
            </div>
            
            <a href='#' class='cta-button' style='color: white; text-decoration: none;'>
                Começar a Explorar 🚀
            </a>
            
            <div class='info-box'>
                <p><strong>💡 Dica:</strong> Mantenha este email para referência futura.</p>
                <p><strong>ID da sua conta:</strong> {userId}</p>
            </div>
            
            <p>Se você tiver alguma dúvida ou precisar de ajuda, nossa equipe de suporte está sempre pronta para ajudar.</p>
            
            <p>Mais uma vez, seja bem-vindo e aproveite ao máximo a FIAP Cloud Games!</p>
            
            <p style='margin-top: 30px;'>
                Atenciosamente,<br>
                <strong>Equipe FIAP Cloud Games</strong>
            </p>
        </div>
        <div class='footer'>
            <p><strong>FIAP Cloud Games</strong></p>
            <p>Sua plataforma de jogos na nuvem</p>
            <div class='social-links'>
                <a href='#'>Suporte</a> | 
                <a href='#'>Termos de Uso</a> | 
                <a href='#'>Privacidade</a>
            </div>
            <p style='margin-top: 20px;'>© {DateTime.Now.Year} FIAP Cloud Games. Todos os direitos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPaymentApprovedTemplate(string userName, Guid orderId, Guid gameId, decimal price)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .success-icon {{ font-size: 48px; text-align: center; margin: 20px 0; }}
        .order-details {{ background-color: white; padding: 20px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #4CAF50; }}
        .detail-row {{ margin: 10px 0; }}
        .detail-label {{ font-weight: bold; color: #666; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Pagamento Aprovado!</h1>
        </div>
        <div class='content'>
            <div class='success-icon'>✓</div>
            <p>Olá <strong>{userName}</strong>,</p>
            <p>Seu pagamento foi processado com sucesso!</p>
            
            <div class='order-details'>
                <h3>Detalhes do Pedido</h3>
                <div class='detail-row'>
                    <span class='detail-label'>Número do Pedido:</span> {orderId}
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>ID do Jogo:</span> {gameId}
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Valor Pago:</span> R$ {price:F2}
                </div>
            </div>
            
            <p>Seu pedido está sendo processado e você receberá mais informações em breve.</p>
            <p>Obrigado por escolher a FIAP Cloud Games!</p>
        </div>
        <div class='footer'>
            <p>FIAP Cloud Games - Todos os direitos reservados</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPaymentRejectedTemplate(string userName, Guid orderId, Guid gameId, decimal price)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .error-icon {{ font-size: 48px; text-align: center; margin: 20px 0; }}
        .order-details {{ background-color: white; padding: 20px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #f44336; }}
        .detail-row {{ margin: 10px 0; }}
        .detail-label {{ font-weight: bold; color: #666; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .action-button {{ display: inline-block; background-color: #2196F3; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Pagamento Rejeitado</h1>
        </div>
        <div class='content'>
            <div class='error-icon'>✗</div>
            <p>Olá <strong>{userName}</strong>,</p>
            <p>Infelizmente, seu pagamento não pôde ser processado.</p>
            
            <div class='order-details'>
                <h3>Detalhes do Pedido</h3>
                <div class='detail-row'>
                    <span class='detail-label'>Número do Pedido:</span> {orderId}
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>ID do Jogo:</span> {gameId}
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Valor:</span> R$ {price:F2}
                </div>
            </div>
            
            <p>Por favor, verifique os dados do seu método de pagamento e tente novamente.</p>
            <p>Se o problema persistir, entre em contato com nosso suporte.</p>
            
            <p>Obrigado por escolher a FIAP Cloud Games!</p>
        </div>
        <div class='footer'>
            <p>FIAP Cloud Games - Todos os direitos reservados</p>
        </div>
    </div>
</body>
</html>";
    }
}
