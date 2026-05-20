using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Postech.Notifications.Api.Infrastructure.MongoDB.Documents;
using Postech.Notifications.Api.Infrastructure.MongoDB.Repositories;

namespace Postech.Notifications.Api.Infrastructure.DynamoDB.Repositories;

public class EventLogDynamoRepository : IEventLogRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public EventLogDynamoRepository(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
    }

    public async Task InsertAsync(EventLogDocument document, CancellationToken cancellationToken = default)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = MapToItem(document)
        };

        await _dynamoDb.PutItemAsync(request, cancellationToken);
    }

    private static Dictionary<string, AttributeValue> MapToItem(EventLogDocument doc)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = doc.Id.ToString() },
            ["EventType"] = new AttributeValue { S = doc.EventType },
            ["Status"] = new AttributeValue { S = doc.Status },
            ["OccurredAt"] = new AttributeValue { S = doc.OccurredAt.ToString("O") },
            ["Attempt"] = new AttributeValue { N = doc.Attempt.ToString() }
        };

        if (doc.CorrelationId.HasValue)
            item["CorrelationId"] = new AttributeValue { S = doc.CorrelationId.Value.ToString() };

        if (doc.ErrorMessage is not null)
            item["ErrorMessage"] = new AttributeValue { S = doc.ErrorMessage };

        if (doc.ErrorType is not null)
            item["ErrorType"] = new AttributeValue { S = doc.ErrorType };

        if (doc.Payload.Count > 0)
        {
            item["Payload"] = new AttributeValue
            {
                M = doc.Payload.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new AttributeValue { S = kvp.Value }
                )
            };
        }

        return item;
    }
}
