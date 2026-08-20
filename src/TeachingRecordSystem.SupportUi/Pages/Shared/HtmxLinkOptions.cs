namespace TeachingRecordSystem.SupportUi.Pages.Shared;

// Opts a navigation link in to swapping part of the page with htmx instead of reloading it.
// Include is a CSS selector for inputs elsewhere on the page whose values should be sent with the
// request - it lets state that only lives in the DOM survive the navigation.
public record HtmxLinkOptions
{
    public required string Select { get; init; }

    public required string Target { get; init; }

    public required string Swap { get; init; }

    public string? Include { get; init; }
}
