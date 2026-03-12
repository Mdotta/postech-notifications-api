# FIAP Cloud Games - Notifications API

Microsserviço responsável pelo envio de notificações (emails) da plataforma FIAP Cloud Games.

## Tecnologias

- .NET 10
- RabbitMQ (via MassTransit)
- Brevo (Sendinblue) - Serviço de envio de emails transacionais
- Docker

## Eventos

### Consome
- `PaymentProcessedEvent`: Recebido do PaymentsAPI quando um pagamento é processado (Aprovado/Rejeitado)

## Configuração

### Variáveis de Ambiente / appsettings.json

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "Brevo": {
    "ApiKey": "sua-api-key-do-brevo",
    "SenderEmail": "noreply@fiapcloudgames.com",
    "SenderName": "FIAP Cloud Games"
  }
}
```

### Obter API Key do Brevo

1. Acesse [https://www.brevo.com](https://www.brevo.com)
2. Crie uma conta ou faça login
3. Vá em **SMTP & API** > **API Keys**
4. Crie uma nova API Key
5. Copie a chave e configure no `appsettings.json` ou variável de ambiente `Brevo:ApiKey`

## Funcionalidades

### Envio de Emails

O serviço consome eventos `PaymentProcessedEvent` do RabbitMQ e envia emails apropriados:

- **Pagamento Aprovado**: Email de confirmação com detalhes do pedido
- **Pagamento Rejeitado**: Email informando sobre a falha no processamento

### Templates de Email

Os templates HTML são gerados dinamicamente e incluem:
- Design responsivo
- Informações do pedido (OrderId, GameId, Valor)
- Mensagens personalizadas baseadas no status do pagamento

## Observações Importantes

⚠️ **TODO**: Atualmente o serviço usa um email mock baseado no `UserId`. Em produção, é necessário:

1. Integrar com um serviço de usuários para buscar o email real do usuário
2. Buscar o preço real do pedido (atualmente o evento não inclui essa informação)
3. Considerar adicionar o email do usuário e o preço ao `PaymentProcessedEvent` no Payments API

## Execução

```bash
dotnet run --project src/Postech.Notifications.Api/Postech.Notifications.Api/Postech.Notifications.Api.csproj
```

## Docker

```bash
docker build -t postech-notifications-api .
docker run -p 5000:8080 postech-notifications-api
```
