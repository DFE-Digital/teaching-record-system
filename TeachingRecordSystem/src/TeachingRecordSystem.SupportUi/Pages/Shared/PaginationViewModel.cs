namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class PaginationViewModel(int? Page, int TotalItems, int ItemsPerPage, string BaseUrl)
{
    public int CurrentPage =>
        Math.Clamp(Page ?? 1, 1, LastPage);

    public int FirstPage => 1;

    public int LastPage =>
        (int)Math.Ceiling(TotalItems / (decimal)ItemsPerPage);

    public int PreviousPage =>
        CurrentPage - 1;

    public int NextPage =>
        CurrentPage + 1;

    public bool ShowPagination =>
        LastPage > 1;

    public bool ShowFirstPage =>
        PreviousPage > FirstPage;

    public bool ShowFirstPageEllipsis =>
        PreviousPage - FirstPage > 1;

    public bool ShowPreviousPage =>
        CurrentPage > FirstPage;

    public bool ShowNextPage =>
        CurrentPage < LastPage;

    public bool ShowLastPageEllipsis =>
        LastPage - NextPage > 1;

    public bool ShowLastPage =>
        NextPage < LastPage;

    public string PageLink(int page) =>
        $"{BaseUrl}?page={page}";

    public IEnumerable<T> Paginate<T>(IEnumerable<T> source) =>
        source
        .Skip((CurrentPage - 1) * ItemsPerPage)
        .Take(ItemsPerPage);
}
