using Postech.Notifications.Api.Infrastructure.MongoDB.Documents;

namespace Postech.Notifications.Api.Infrastructure.MongoDB.Repositories;

public interface IEventLogRepository
{
    Task InsertAsync(EventLogDocument document, CancellationToken cancellationToken = default);
}
