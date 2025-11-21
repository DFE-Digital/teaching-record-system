using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.WebCommon.FormFlow;

public interface IJourneyWithSteps
{
    static virtual string? ReturnUrlQueryParameterName { get; } = "returnUrl";

    JourneySteps Steps { get; }
}

public static class JourneyInstanceWithStepsExtensions
{
    public static string? GetBackLinkUrl<TState>(this JourneyInstance<TState> journeyInstance)
        where TState : IJourneyWithSteps
    {
        var httpContext = GetHttpContext(journeyInstance);
        var currentStep = GetCurrentStep(httpContext, journeyInstance);

        if (GetReturnUrl(httpContext, journeyInstance) is { } returnUrl)
        {
            return returnUrl;
        }

        return journeyInstance.State.Steps.GetPreviousStep(currentStep)?.StepUrl;
    }

    public static string? GetPreviousStepUrl<TState>(this JourneyInstance<TState> journeyInstance)
        where TState : IJourneyWithSteps
    {
        var httpContext = GetHttpContext(journeyInstance);
        var currentStep = GetCurrentStep(httpContext, journeyInstance);

        return journeyInstance.State.Steps.GetPreviousStep(currentStep)?.StepUrl;
    }

    public static async Task<IActionResult> UpdateStateAndRedirectToNextStepAsync<TState>(
        this JourneyInstance<TState> journeyInstance,
        Action<TState> update,
        string nextStepUrl)
        where TState : IJourneyWithSteps
    {
        var httpContext = GetHttpContext(journeyInstance);
        var currentStep = GetCurrentStep(httpContext, journeyInstance);

        await journeyInstance.UpdateStateAsync(state =>
        {
            update(state);
            state.Steps.AddStep(currentStep, new JourneyStep(nextStepUrl));
        });

        // If we have a returnUrl and the URL for that step is valid, redirect there instead
        if (GetReturnUrl(httpContext, journeyInstance) is { } returnUrl)
        {
            return new RedirectResult(returnUrl);
        }

        return new RedirectResult(journeyInstance.State.Steps.LastStepUrl);
    }

    public static JourneyStep CreateStepForCurrentRequest(this HttpContext httpContext)
    {
        return new(httpContext.Request.GetEncodedPathAndQuery());
    }

    private static HttpContext GetHttpContext<TState>(JourneyInstance<TState> journeyInstance)
        where TState : IJourneyWithSteps
    {
        return (journeyInstance.Properties[typeof(ActionContext)] as ActionContext)?.HttpContext
            ?? throw new InvalidOperationException("JourneyInstance does not contain an ActionContext.");
    }

    private static JourneyStep GetCurrentStep<TState>(HttpContext httpContext, JourneyInstance<TState> journeyInstance)
        where TState : IJourneyWithSteps
    {
        if (!TryGetCurrentStep(httpContext, journeyInstance, out var currentStep))
        {
            throw new InvalidOperationException("Current page is not a valid step for the current journey.");
        }

        return currentStep;
    }

    private static string? GetReturnUrl<TState>(HttpContext httpContext, JourneyInstance<TState> journeyInstance)
        where TState : IJourneyWithSteps
    {
        return TState.ReturnUrlQueryParameterName is { } returnUrlQueryParameterName &&
            httpContext.Request.Query[returnUrlQueryParameterName].FirstOrDefault() is { } returnUrl &&
            journeyInstance.State.Steps.ContainsStep(new JourneyStep(returnUrl)) ? returnUrl : null;
    }

    private static bool TryGetCurrentStep<TState>(
        HttpContext httpContext,
        JourneyInstance<TState> journeyInstance,
        [NotNullWhen(true)] out JourneyStep? currentStep)
        where TState : IJourneyWithSteps
    {
        currentStep = CreateStepForCurrentRequest(httpContext);
        return journeyInstance.State.Steps.ContainsStep(currentStep);
    }
}
