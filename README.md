# Notifications API — Microsservico de Notificacoes (FCG)

Microsservico de **Notificacoes** da FIAP Cloud Games (Tech Challenge). Responsavel pelo envio de emails transacionais e registro de eventos.

## Finalidade

- **Consome `UserCreatedEvent`** — envia email de boas-vindas ao novo usuario.
- **Consome `PaymentProcessedEvent`** — envia email de confirmacao (aprovado) ou rejeicao de pagamento.
- **Registra logs de eventos** — persiste cada evento processado em MongoDB (local) ou DynamoDB (producao AWS) para auditoria.

> O servico e implantado de duas formas:
> - **Local (dev):** ASP.NET com MassTransit + RabbitMQ como consumidor.
> - **AWS (producao):** AWS Lambda com trigger SQS (imagem container).

## Tecnologias / Dependencias

| Recurso | Local (dev) | AWS (producao) |
|---------|------------|----------------|
| Runtime | .NET 10 | .NET 8 (Lambda) |
| Mensageria | MassTransit + RabbitMQ | SQS (Lambda trigger) |
| Event log | MongoDB 7 | DynamoDB |
| Email | Brevo (Sendinblue) | Brevo |
| Logs | Console / arquivo | CloudWatch Logs |

Pacotes NuGet principais: `MassTransit.RabbitMQ`, `MongoDB.Driver`, `AWSSDK.DynamoDBv2`, `Serilog.AspNetCore`, `Scalar.AspNetCore`.

## Como rodar localmente

```bash
# 1. Subir dependencias (RabbitMQ, MongoDB)
cd ../postech-orchestration/docker
docker compose up -d rabbitmq mongodb

# 2. Configurar chave Brevo (opcional — mock email funciona sem)
export Brevo__ApiKey="sua-api-key"

# 3. Rodar a API
cd ../../postech-notifications-api/src/Postech.Notifications.Api
dotnet run
```

- `http://localhost:{port}/health` — health check
- `http://localhost:{port}/scalar/v1` — documentacao (development)

## Variaveis de ambiente

| Variavel | Descricao | Default (local) |
|----------|-----------|-----------------|
| `RabbitMQ__Host` | RabbitMQ (local dev) | `localhost` |
| `RabbitMQ__Port` | Porta RabbitMQ | `5672` |
| `RabbitMQ__Username` | Usuario RabbitMQ | `guest` |
| `RabbitMQ__Password` | Senha RabbitMQ | `guest` |
| `MongoDB__ConnectionString` | MongoDB (local) | `mongodb://localhost:27017` |
| `MongoDB__DatabaseName` | MongoDB database | `postech_notifications` |
| `DynamoDB__UseDynamoDB` | Usar DynamoDB em vez de MongoDB | `false` |
| `DynamoDB__TableName` | Nome da tabela DynamoDB | `postech_notifications_event_logs` |
| `Brevo__ApiKey` | API Key Brevo | — |
| `Brevo__SenderEmail` | Email remetente | `fiapcloudgamesgrupo25@gmail.com` |
| `Brevo__SenderName` | Nome remetente | `FIAP Cloud Games` |

## Endpoints

| Metodo | Rota | Descricao |
|--------|------|-----------|
| `GET` | `/health` | Health check |

## Eventos

- **Consome `UserCreatedEvent`** (UserId, Email, Name) — envia email de boas-vindas.
- **Consome `PaymentProcessedEvent`** (OrderId, UserId, GameId, IsSuccessful) — envia email de status do pagamento.

Ambos persistem um `EventLogDocument` com status (Success/Failed), payload, correlation ID e mensagem de erro (se houver) no document store.

## Estrutura do projeto

```
src/Postech.Notifications.Api/
  Application/
    Consumers/            # UserCreatedConsumer, OrderProcessedConsumer (MassTransit)
    Services/             # IEmailService, MockEmailService, BrevoEmailProvider
  Infrastructure/
    DynamoDB/             # DynamoDbSettings, EventLogDynamoRepository
    Email/                # Implementacoes de envio de email (mock e Brevo)
    MongoDB/              # MongoDbSettings, EventLogDocument, EventLogRepository
    MassTransit/          # Configuracao MassTransit + RabbitMQ

src/Postech.Notifications.Lambda/   # Variante Lambda para deploy AWS
    Dockerfile           # Dockerfile especifico para Lambda (runtime dotnet:8)
    Function.cs          # Handler da funcao Lambda
```

## Como atualizar imagem no ECR

**Imagem Lambda** (usada em producao):

```bash
ACCOUNT=$(aws sts get-caller-identity --query Account --output text)
ECR="${ACCOUNT}.dkr.ecr.us-east-1.amazonaws.com/tf-postech-postech-notifications-lambda"
LAMBDA_DIR="src/Postech.Notifications.Lambda"

aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin "${ACCOUNT}.dkr.ecr.us-east-1.amazonaws.com"

docker build -t "${ECR}:latest" -f "${LAMBDA_DIR}/Dockerfile" "${LAMBDA_DIR}"
docker push "${ECR}:latest"
```

A Lambda sera atualizada automaticamente ao apontar para a nova tag `:latest`. Para forcar a atualizacao imediata:

```bash
aws lambda update-function-code --function-name tf-postech-notifications-user-created --image-uri "${ECR}:latest"
aws lambda update-function-code --function-name tf-postech-notifications-order-processed --image-uri "${ECR}:latest"
```
