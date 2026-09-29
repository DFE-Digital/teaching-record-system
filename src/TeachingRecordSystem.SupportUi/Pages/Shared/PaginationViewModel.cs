namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class PaginationViewModel(int currentPage, int lastPage, Func<int, string> getPageLink)
{
    public int CurrentPage => currentPage;
    public int LastPage => lastPage;

    public string GetPageLink(int page) => getPageLink(page);

    public static PaginationViewModel Create<T>(ResultPage<T> result, Func<int, string> getPageLink)
    {
        return new(result.CurrentPage, result.LastPage, getPageLink);
    }
}
