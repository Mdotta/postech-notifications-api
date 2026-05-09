using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Postech.Notifications.Api.Infrastructure.MongoDB.Documents;

public class EventLogDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Tipo do evento: "OrderProcessed", "UserCreated"
    public string EventType { get; set; } = string.Empty;

    // "Success", "Failed"
    public string Status { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public Guid? CorrelationId { get; set; }

    // Dados do evento (orderId, userId, gameId, etc.)
    public Dictionary<string, string> Payload { get; set; } = [];

    // Preenchido apenas em caso de falha
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }

    // Número da tentativa (útil para ver retries antes de ir pra DLQ)
    public int Attempt { get; set; } = 1;
}
