using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.WebCommon.FormFlow;

[method: JsonConstructor]
public record JourneyStep(string StepId, string StepUrl)
{
    public JourneyStep(string stepUrl) :
        this(CreateStepIdFromRelativeUrl(stepUrl), stepUrl)
    {
    }

    private static string CreateStepIdFromRelativeUrl(string url) => url.Split('?')[0];
}

[JsonConverter(typeof(JourneyStepsJsonConverter))]
public class JourneySteps(IEnumerable<JourneyStep> steps)
{
    private readonly List<JourneyStep> _steps = steps.ToList();

    public JourneySteps(params IEnumerable<string> stepUrls)
        : this(stepUrls.Select(url => new JourneyStep(url)))
    {
    }

    public string LastStepUrl => _steps.Last().StepUrl;

    public IReadOnlyCollection<JourneyStep> Steps => _steps.AsReadOnly();

    public static JourneySteps Create(JourneyStep firstStep) => new([firstStep]);

    public void AddStep(JourneyStep currentStep, JourneyStep nextStep, bool removeAfter = false, bool removeBefore = false)
    {
        var currentStepIndex = FindStep(currentStep);

        if (currentStepIndex == -1)
        {
            throw new InvalidOperationException("The specified current step is not in the journey steps.");
        }

        var nextStepIndex = FindStep(nextStep);

        if (removeAfter || nextStepIndex == -1)
        {
            _steps.RemoveRange(currentStepIndex + 1, _steps.Count - (currentStepIndex + 1));
        }

        if (removeBefore)
        {
            _steps.RemoveRange(0, currentStepIndex);
        }

        if (nextStepIndex == -1)
        {
            _steps.Add(nextStep);
        }
    }

    public bool ContainsStep(JourneyStep step) => FindStep(step) > -1;

    public JourneyStep? GetPreviousStep(JourneyStep currentStep)
    {
        var currentStepIndex = FindStep(currentStep);

        if (currentStepIndex == -1)
        {
            throw new InvalidOperationException("The specified current step is not in the journey steps.");
        }

        return currentStepIndex > 0 ? _steps[currentStepIndex - 1] : null;
    }

    private int FindStep(JourneyStep step) => _steps.FindIndex(0, s => s.StepId == step.StepId);
}

internal class JourneyStepsJsonConverter : JsonConverter<JourneySteps>
{
    public override JourneySteps? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var wrapper = JsonSerializer.Deserialize<StepsWrapper>(ref reader, options);
        return wrapper is not null ? new JourneySteps(wrapper.Steps.ToArray()) : null;
    }

    public override void Write(Utf8JsonWriter writer, JourneySteps value, JsonSerializerOptions options)
    {
        var wrapper = new StepsWrapper { Steps = value.Steps };
        JsonSerializer.Serialize(writer, wrapper, options);
    }

    private class StepsWrapper
    {
        public IEnumerable<JourneyStep> Steps { get; set; } = [];
    }
}
