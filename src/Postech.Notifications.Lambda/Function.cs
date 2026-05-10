using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Postech.Notifications.Lambda;

public class Function
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        var failedIds = new List<SQSBatchResponse.BatchItemFailure>();

        foreach (var record in evnt.Records)
        {
            try
            {
                context.Logger.LogInformation("Processing message {MessageId}", record.MessageId);

                var eventType = record.MessageAttributes.ContainsKey("EventType")
                    ? record.MessageAttributes["EventType"].StringValue
                    : null;

                switch (eventType)
                {
                    case "UserCreatedEvent":
                        var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(record.Body, JsonOptions);
                        context.Logger.LogInformation("[MOCK] Welcome email sent to {Email}", userEvent?.Email);
                        break;

                    case "OrderProcessedEvent":
                        var orderEvent = JsonSerializer.Deserialize<OrderProcessedEvent>(record.Body, JsonOptions);
                        if (orderEvent?.IsSuccessful == true)
                            context.Logger.LogInformation("[MOCK] Payment approved email sent for order {OrderId}", orderEvent.OrderId);
                        else
                            context.Logger.LogInformation("[MOCK] Payment rejected email sent for order {OrderId}", orderEvent.OrderId);
                        break;

                    default:
                        context.Logger.LogWarning("Unknown event type: {EventType}", eventType);
                        break;
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError("Error processing message {MessageId}: {Error}", record.MessageId, ex.Message);
                failedIds.Add(new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId });
            }
        }

        return new SQSBatchResponse(failedIds);
    }
}

public class UserCreatedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class OrderProcessedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
}
