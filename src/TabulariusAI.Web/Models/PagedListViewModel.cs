namespace TabulariusAI.Web.Models;

/// <summary>
/// Represents a reusable server-side paginated list with its active search criteria.
/// </summary>
/// <typeparam name="T">The row type displayed by the list.</typeparam>
public sealed class PagedListViewModel<T>
{
    /// <summary>Gets or sets the rows in the current page.</summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>Gets or sets the total number of rows matching the current filters.</summary>
    public int TotalItems { get; set; }

    /// <summary>Gets or sets the current one-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Gets or sets the number of rows requested per page.</summary>
    public int PageSize { get; set; } = 25;

    /// <summary>Gets or sets the active free-text search value.</summary>
    public string? Search { get; set; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));

    /// <summary>Gets whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Gets whether a following page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Gets the one-based index of the first row shown on the current page.</summary>
    public int FirstItem => TotalItems == 0 ? 0 : ((Page - 1) * PageSize) + 1;

    /// <summary>Gets the one-based index of the last row shown on the current page.</summary>
    public int LastItem => Math.Min(Page * PageSize, TotalItems);
}
