using Prometheus;

namespace Postech.Notifications.Api.Infrastructure.Metrics;

public static class NotificationsMetrics
{
    public static readonly Counter EmailsSent = Prometheus.Metrics.CreateCounter(
        "emails_sent_total", "Emails sent by the notification service",
        new CounterConfiguration { LabelNames = ["type"] });

    public static readonly Counter EventLogsPersisted = Prometheus.Metrics.CreateCounter(
        "event_logs_persisted_total", "Event logs persisted to document store",
        new CounterConfiguration { LabelNames = ["status"] });
}
