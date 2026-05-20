namespace Postech.Notifications.Api.Infrastructure.DynamoDB;

public class DynamoDbSettings
{
    public bool UseDynamoDB { get; set; }
    public string TableName { get; set; } = "postech_notifications_event_logs";
}
