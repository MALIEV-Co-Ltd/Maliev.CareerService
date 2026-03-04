
using System.Diagnostics.Metrics;

namespace Maliev.CareerService.Infrastructure.Services;

/// <summary>
/// Service for managing OpenTelemetry metrics
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Increments the job applications counter
    /// </summary>
    void IncrementJobApplications(string status);

    /// <summary>
    /// Increments the training enrollments counter
    /// </summary>
    void IncrementTrainingEnrollments(string status);

    /// <summary>
    /// Sets the active job postings gauge value
    /// </summary>
    void SetActiveJobPostings(int count);

    /// <summary>
    /// Increments the concurrent users gauge
    /// </summary>
    void IncrementConcurrentUsers();

    /// <summary>
    /// Decrements the concurrent users gauge
    /// </summary>
    void DecrementConcurrentUsers();
}

/// <summary>
/// Implementation of metrics service using System.Diagnostics.Metrics (OpenTelemetry)
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly Meter _meter;
    private readonly Counter<long> _jobApplicationsCounter;
    private readonly Counter<long> _trainingEnrollmentsCounter;
    private readonly ObservableGauge<long> _activeJobPostingsGauge;
    private readonly ObservableGauge<long> _concurrentUsersGauge;

    // Backing fields reported by observable gauges
    private long _activeJobPostings;
    private long _concurrentUsers;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsService"/> class.
    /// </summary>
    public MetricsService()
    {
        _meter = new Meter("careers", "1.0.0");

        // Counter: Total job applications by status
        _jobApplicationsCounter = _meter.CreateCounter<long>(
            "career_job_applications_total",
            description: "Total number of job applications submitted");

        // Counter: Total training enrollments by status
        _trainingEnrollmentsCounter = _meter.CreateCounter<long>(
            "career_training_enrollments_total",
            description: "Total number of training program enrollments");

        // Observable Gauge: Current number of active job postings
        _activeJobPostingsGauge = _meter.CreateObservableGauge<long>(
            "career_active_job_postings",
            observeValues: () => new[]
            {
                new Measurement<long>(Interlocked.Read(ref _activeJobPostings))
            },
            description: "Current number of active job postings");

        // Observable Gauge: Current number of concurrent users
        _concurrentUsersGauge = _meter.CreateObservableGauge<long>(
            "career_concurrent_users",
            observeValues: () => new[]
            {
                new Measurement<long>(Interlocked.Read(ref _concurrentUsers))
            },
            description: "Current number of concurrent API users");
    }

    /// <summary>
    /// Performs the IncrementJobApplications operation
    /// </summary>
    /// <param name="status">The status</param>
    public void IncrementJobApplications(string status)
    {
        _jobApplicationsCounter.Add(
            1,
            new[] { new KeyValuePair<string, object?>("status", status) });
    }

    /// <summary>
    /// Performs the IncrementTrainingEnrollments operation
    /// </summary>
    /// <param name="status">The status</param>
    public void IncrementTrainingEnrollments(string status)
    {
        _trainingEnrollmentsCounter.Add(
            1,
            new[] { new KeyValuePair<string, object?>("status", status) });
    }

    /// <summary>
    /// Sets ActiveJobPostings
    /// </summary>
    /// <param name="count">The count</param>
    public void SetActiveJobPostings(int count)
    {
        Interlocked.Exchange(ref _activeJobPostings, count);
    }

    /// <summary>
    /// Performs the IncrementConcurrentUsers operation
    /// </summary>
    public void IncrementConcurrentUsers()
    {
        Interlocked.Increment(ref _concurrentUsers);
    }

    /// <summary>
    /// Performs the DecrementConcurrentUsers operation
    /// </summary>
    public void DecrementConcurrentUsers()
    {
        Interlocked.Decrement(ref _concurrentUsers);
    }
}
