using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace TeachingRecordSystem.UiCommon.FormFlow;

internal sealed class ActionJourneyMetadata(string journeyName)
{
    public string JourneyName { get; } = journeyName;
}

internal static class ActionContextExtensions
{
    public static ActionJourneyMetadata? GetActionJourneyMetadata(this ActionContext context)
    {
        return context.ActionDescriptor.GetProperty<ActionJourneyMetadata>();
    }
}
