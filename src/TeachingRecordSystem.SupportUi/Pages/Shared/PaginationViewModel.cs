namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class PaginationViewModel(int currentPage, int lastPage, Func<int, string> getPageLink)
{
    public int CurrentPage => currentPage;
    public int FirstPage => 1;
    public int LastPage => lastPage;

    public bool ShowPagination =>
        LastPage > 1;

    public int PreviousPage =>
        CurrentPage - 1;

    public bool ShowPreviousPage =>
        CurrentPage > FirstPage;

    public int NextPage =>
        CurrentPage + 1;

    public bool ShowNextPage =>
        CurrentPage < LastPage;

    public bool ShowFirstPage =>
        PreviousPage > FirstPage;

    public bool ShowFirstPageEllipsis =>
        PreviousPage - FirstPage > 1;

    public bool ShowLastPage =>
        NextPage < LastPage;

    public bool ShowLastPageEllipsis =>
        LastPage - NextPage > 1;

    public string GetPageLink(int page) => getPageLink(page);

    public static PaginationViewModel Create<T>(ResultPage<T> result, Func<int, string> getPageLink)
    {
        return new(result.CurrentPage, result.LastPage, getPageLink);
    }
}
