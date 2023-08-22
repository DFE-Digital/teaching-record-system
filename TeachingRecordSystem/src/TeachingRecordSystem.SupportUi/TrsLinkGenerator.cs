namespace TeachingRecordSystem.SupportUi;

public class TrsLinkGenerator
{
    private readonly LinkGenerator _linkGenerator;

    public TrsLinkGenerator(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public string Index() => GetRequiredPathByPage("/Index");

    public string SignOut() => GetRequiredPathByPage("/SignOut");

    public string SignedOut() => GetRequiredPathByPage("/SignedOut");

    public string Cases() => GetRequiredPathByPage("/Cases/Index");

    public string EditCase(string caseId) => GetRequiredPathByPage("/Cases/EditCase/Index", new { caseId = caseId });

    public string Users() => GetRequiredPathByPage("/Users/Index");

    public string AddUser() => GetRequiredPathByPage("/Users/AddUser/Index");

    public string AddUser(string userId) => GetRequiredPathByPage("/Users/AddUser/Confirm", new { userId = userId });

    public string EditUser(Guid userId) => GetRequiredPathByPage("/Users/EditUser", new { userId });

    private string GetRequiredPathByPage(string page, object? routeValues = null) =>
        _linkGenerator.GetPathByPage(page, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");
}
