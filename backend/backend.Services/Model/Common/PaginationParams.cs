namespace backend.Services.Model.Common;

/// <summary>
/// The single paging input for every list endpoint. Endpoints needing extra
/// filters inherit this rather than re-declaring the page fields.
/// </summary>
public class PaginationParams
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }
}
