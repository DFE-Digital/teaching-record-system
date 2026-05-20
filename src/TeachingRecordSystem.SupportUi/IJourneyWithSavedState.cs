using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeachingRecordSystem.SupportUi;

public interface IJourneyWithSavedState
{
    SavedJourneyState? SavedJourneyState { get; set; }
}

public static class JourneyWithSavedStateExtensions
{
    public static void ApplySavedModelStateValues(this IJourneyWithSavedState journeyState, string pageName, ModelStateDictionary modelState)
    {
        if (journeyState.SavedJourneyState?.PageName != pageName)
        {
            return;
        }

        foreach (var (key, value) in journeyState.SavedJourneyState.ModelStateValues)
        {
            modelState.SetModelValue(key, value, value);
        }
    }

    public static void ClearSavedModelStateValues(this IJourneyWithSavedState journeyState, string pageName)
    {
        if (journeyState.SavedJourneyState?.PageName != pageName)
        {
            return;
        }

        journeyState.SavedJourneyState = null;
    }
}
