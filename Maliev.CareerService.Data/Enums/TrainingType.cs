namespace Maliev.CareerService.Data.Enums;

/// <summary>
/// Represents the delivery method or format of a training course.
/// </summary>
public enum TrainingType
{
    /// <summary>
    /// In-person classroom or workshop training
    /// </summary>
    InPerson = 0,

    /// <summary>
    /// Online/virtual training with instructor
    /// </summary>
    Online = 1,

    /// <summary>
    /// Self-paced online course
    /// </summary>
    SelfPaced = 2,

    /// <summary>
    /// Hands-on workshop or lab session
    /// </summary>
    Workshop = 3,

    /// <summary>
    /// Professional certification program
    /// </summary>
    Certification = 4,

    /// <summary>
    /// External training provided by third party
    /// </summary>
    External = 5
}
