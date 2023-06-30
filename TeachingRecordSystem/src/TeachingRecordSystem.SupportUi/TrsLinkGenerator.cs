namespace TeachingRecordSystem.SupportUi;

public class TrsLinkGenerator
{
    private readonly LinkGenerator _linkGenerator;

    public TrsLinkGenerator(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public string Index() => _linkGenerator.GetPathByPage("/Index") ?? throw GetPageNotFoundException();

    public string SignOut() => _linkGenerator.GetPathByPage("/SignOut") ?? throw GetPageNotFoundException();

    public string SignedOut() => _linkGenerator.GetPathByPage("/SignedOut") ?? throw GetPageNotFoundException();

    private static Exception GetPageNotFoundException() =>
        new InvalidOperationException("Page was not found.");
}
