using Prometheus;

namespace Maliev.CareerService.Api.Services;

/// <summary>
/// Service for managing Prometheus metrics
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
/// Implementation of Prometheus metrics service
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly Counter _jobApplicationsCounter;
    private readonly Counter _trainingEnrollmentsCounter;
    private readonly Gauge _activeJobPostingsGauge;
    private readonly Gauge _concurrentUsersGauge;

    public MetricsService()
    {
        // Counter: Total job applications by status
        _jobApplicationsCounter = Metrics.CreateCounter(
            "career_job_applications_total",
            "Total number of job applications submitted",
            new CounterConfiguration
            {
                LabelNames = ["status"]
            });

        // Counter: Total training enrollments by status
        _trainingEnrollmentsCounter = Metrics.CreateCounter(
            "career_training_enrollments_total",
            "Total number of training program enrollments",
            new CounterConfiguration
            {
                LabelNames = ["status"]
            });

        // Gauge: Current number of active job postings
        _activeJobPostingsGauge = Metrics.CreateGauge(
            "career_active_job_postings",
            "Current number of active job postings");

        // Gauge: Current number of concurrent users
        _concurrentUsersGauge = Metrics.CreateGauge(
            "career_concurrent_users",
            "Current number of concurrent API users");
    }

    public void IncrementJobApplications(string status)
    {
        _jobApplicationsCounter.WithLabels(status).Inc();
    }

    public void IncrementTrainingEnrollments(string status)
    {
        _trainingEnrollmentsCounter.WithLabels(status).Inc();
    }

    public void SetActiveJobPostings(int count)
    {
        _activeJobPostingsGauge.Set(count);
    }

    public void IncrementConcurrentUsers()
    {
        _concurrentUsersGauge.Inc();
    }

    public void DecrementConcurrentUsers()
    {
        _concurrentUsersGauge.Dec();
    }
}
