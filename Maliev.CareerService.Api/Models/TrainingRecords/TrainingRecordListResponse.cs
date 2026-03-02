namespace Maliev.CareerService.Api.Models.TrainingRecords;

/// <summary>
/// Paginated list response for training records (Feature 003)
/// </summary>
public class TrainingRecordListResponse
{
    /// <summary>List of training records in this page</summary>
    public List<TrainingRecordResponse> Items { get; set; } = [];

    /// <summary>Current page number</summary>
    public int Page { get; set; }

    /// <summary>Number of items per page</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of training records</summary>
    public int TotalCount { get; set; }

    /// <summary>Total number of pages</summary>
    public int TotalPages { get; set; }
}
