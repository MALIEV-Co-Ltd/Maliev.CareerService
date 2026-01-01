namespace Maliev.CareerService.Data.Enums;

/// <summary>
/// Represents the current status of a training record.
/// </summary>
public enum TrainingStatus
{
    /// <summary>
    /// Training has been completed successfully
    /// </summary>
    Completed = 0,

    /// <summary>
    /// Training is currently in progress
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Training has not been started
    /// </summary>
    NotStarted = 2,

    /// <summary>
    /// Certification has expired and requires renewal
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Training was attempted but not completed successfully
    /// </summary>
    Failed = 4
}
