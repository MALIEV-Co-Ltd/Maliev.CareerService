namespace Maliev.CareerService.Api.Models;
/// <summary>
/// Represents a PagedResult
/// </summary>

public class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the list of items in the current page.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();
    /// <summary>
    /// Gets or sets the total count of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int Page { get; set; }
    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPrevious => Page > 1;
    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNext => Page < TotalPages;
}
