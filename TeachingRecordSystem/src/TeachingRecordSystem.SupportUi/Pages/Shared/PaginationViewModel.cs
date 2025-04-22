using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class PaginationViewModel(string name, int currentPage, int firstPage, int lastPage, IQueryCollection currentRequestQueryString)
{
    public int CurrentPage => currentPage;
    public int FirstPage => firstPage;
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

    public string PageLink(int page)
    {
        var queryString = new Dictionary<string, StringValues>(currentRequestQueryString);

        queryString[name] = page.ToString();

        return QueryString.Create(queryString).ToString();
    }

    public static PaginationViewModel Create(Pagination pagination, IQueryCollection currentRequestQueryString)
    {
        return new(pagination.Name, pagination.CurrentPage, pagination.FirstPage, pagination.LastPage, currentRequestQueryString);
    }
}
