namespace CloudReportsBuildingBlocksPOC.Configuration;

/// <summary>
/// Pub sub topic and subscriber related configuration options, including retry and circuit breaker settings.
/// </summary>
public class PubSubOptions
{
    public const string SectionName = "PubSub";

    public string FileEventsSubscription { get; set; }

    public string FileEventsTopic { get; set; }

    public string ProjectId { get; set; } = string.Empty;

    public RetryOptions Retry { get; set; } = new ();

    public CircuitBreakerOptions CircuitBreaker { get; set; } = new ();

    public SubscriberOptions Subscriber { get; set; } = new ();
}

/// <summary>
/// Retry configuration for pubsub topics, including max retry attempts, initial delay, and max delay.
/// </summary>
public class RetryOptions
{
    public int MaxRetryAttempts { get; set; } = 3;

    public int InitialDelayMilliseconds { get; set; } = 100;

    public int MaxDelaySeconds { get; set; } = 30;
}

/// <summary>
/// Circuit breaker configuration for pubsub topics, including failure threshold, duration of break, and sampling duration.
/// </summary>
public class CircuitBreakerOptions
{
    public int FailureThreshold { get; set; } = 5;

    public int DurationOfBreakSeconds { get; set; } = 30;

    public int SamplingDurationSeconds { get; set; } = 60;
}

/// <summary>
/// Subscriber options for a topic, including max concurrent messages, ack deadline, max ack extension, and message ordering settings.
/// </summary>
public class SubscriberOptions
{
    public int MaxConcurrentMessages { get; set; } = 10;

    public int AckDeadlineSeconds { get; set; } = 60;

    public int MaxAckExtensionSeconds { get; set; } = 600;

    public bool EnableMessageOrdering { get; set; } = false;
}