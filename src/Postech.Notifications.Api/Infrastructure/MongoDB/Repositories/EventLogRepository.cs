using MongoDB.Driver;
using Postech.Notifications.Api.Infrastructure.MongoDB.Documents;

namespace Postech.Notifications.Api.Infrastructure.MongoDB.Repositories;

public class EventLogRepository : IEventLogRepository
{
    private readonly IMongoCollection<EventLogDocument> _collection;

    public EventLogRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<EventLogDocument>("event_logs");
    }

    public async Task InsertAsync(EventLogDocument document, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}
