namespace TeachingRecordSystem.SupportUi.Endpoints;

public class FilesLinkGenerator(LinkGenerator linkGenerator)
{
    public string File(string fileName, string fileUrl)
    {
        return linkGenerator.GetPathByName("Files", values: new { fileName, fileUrl })
            ?? throw new InvalidOperationException("Failed to generate file link.");
    }
}
