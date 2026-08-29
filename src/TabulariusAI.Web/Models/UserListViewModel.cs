namespace TabulariusAI.Web.Models;

/// <summary>Represents the paginated administrator user list and its active filters.</summary>
public sealed class UserListViewModel
{
    /// <summary>Gets or sets the paginated user rows.</summary>
    public PagedListViewModel<UserListItemViewModel> List { get; set; } = new();

    /// <summary>Gets or sets the selected localized-independent application role filter.</summary>
    public string? Role { get; set; }

    /// <summary>Gets or sets the selected account state filter.</summary>
    public string? Status { get; set; }
}
