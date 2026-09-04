using System.Diagnostics.Metrics;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal sealed class OutboxMetrics
{
    public const string MeterName = "RouteFlow.Deliveries";

    private readonly Counter<long> _processedMessages;
    private readonly Counter<long> _failedMessages;
    private readonly Counter<long> _processorErrors;
    private readonly Histogram<double> _batchDuration;
    private readonly Histogram<int> _batchSize;
    private readonly Histogram<double> _messageAge;

    public OutboxMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _processedMessages = meter.CreateCounter<long>(
            "routeflow.deliveries.outbox.messages.processed",
            unit: "{message}");
        _failedMessages = meter.CreateCounter<long>(
            "routeflow.deliveries.outbox.messages.failed",
            unit: "{message}");
        _processorErrors = meter.CreateCounter<long>(
            "routeflow.deliveries.outbox.processor.errors",
            unit: "{error}");
        _batchDuration = meter.CreateHistogram<double>(
            "routeflow.deliveries.outbox.batch.duration",
            unit: "s");
        _batchSize = meter.CreateHistogram<int>(
            "routeflow.deliveries.outbox.batch.size",
            unit: "{message}");
        _messageAge = meter.CreateHistogram<double>(
            "routeflow.deliveries.outbox.message.age",
            unit: "s");
    }

    public void RecordMessageProcessed() => _processedMessages.Add(1);

    public void RecordMessageFailed() => _failedMessages.Add(1);

    public void RecordProcessorError() => _processorErrors.Add(1);

    public void RecordBatchDuration(TimeSpan duration) => _batchDuration.Record(duration.TotalSeconds);

    public void RecordBatchSize(int count) => _batchSize.Record(count);

    public void RecordMessageAge(TimeSpan age) => _messageAge.Record(Math.Max(0, age.TotalSeconds));
}
