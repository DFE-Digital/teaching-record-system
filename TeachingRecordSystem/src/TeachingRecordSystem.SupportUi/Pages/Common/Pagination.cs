namespace TeachingRecordSystem.SupportUi.Pages.Common;

public class Pagination(string name, int itemsPerPage, IQueryCollection currentRequestQueryString)
{
    private readonly int _page = int.TryParse(currentRequestQueryString[name], out int parsed) ? parsed : 1;
    private int _totalItems;

    public string Name => name;

    public int FirstPage => 1;

    public int LastPage =>
        Math.Max(1, (int)Math.Ceiling(_totalItems / (decimal)itemsPerPage));

    public int CurrentPage =>
       Math.Clamp(_page, FirstPage, LastPage);

    public async Task<T[]> PaginateAsync<T>(IQueryable<T> source, int totalItems)
    {
        _totalItems = totalItems;

        return await source
            .Skip((CurrentPage - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToArrayAsync();
    }
}
