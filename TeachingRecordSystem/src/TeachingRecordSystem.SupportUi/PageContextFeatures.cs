using Microsoft.AspNetCore.Http.Features;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi;

public static class HttpContextExtensions
{
    public static CurrentPersonFeature GetCurrentPersonFeature(this HttpContext context) =>
        context.Features.GetRequiredFeature<CurrentPersonFeature>();

    public static void SetCurrentPersonFeature(this HttpContext context, CurrentPersonFeature currentPersonInfo) =>
        context.Features.Set(currentPersonInfo);

    public static void SetCurrentPersonFeature(this HttpContext context, Person person) =>
        SetCurrentPersonFeature(
            context,
            new CurrentPersonFeature(
                person.PersonId,
                person.FirstName,
                person.MiddleName,
                person.LastName));

    public static CurrentMandatoryQualificationFeature GetCurrentMandatoryQualificationFeature(this HttpContext context) =>
        context.Features.GetRequiredFeature<CurrentMandatoryQualificationFeature>();

    public static void SetCurrentMandatoryQualificationFeature(this HttpContext context, CurrentMandatoryQualificationFeature currentMandatoryQualificationFeature) =>
        context.Features.Set(currentMandatoryQualificationFeature);

    public static CurrentProfessionalStatusFeature GetCurrentProfessionalStatusFeature(this HttpContext context) =>
        context.Features.GetRequiredFeature<CurrentProfessionalStatusFeature>();

    public static void SetCurrentProfessionalStatusFeature(this HttpContext context, CurrentProfessionalStatusFeature currentProfessionalStatusFeature) =>
        context.Features.Set(currentProfessionalStatusFeature);

    public static CurrentSupportTaskFeature GetCurrentSupportTaskFeature(this HttpContext context) =>
        context.Features.GetRequiredFeature<CurrentSupportTaskFeature>();

    public static void SetCurrentSupportTaskFeature(this HttpContext context, CurrentSupportTaskFeature currentSupportTaskFeature) =>
        context.Features.Set(currentSupportTaskFeature);

    public static CurrentAlertFeature GetCurrentAlertFeature(this HttpContext context) =>
        context.Features.GetRequiredFeature<CurrentAlertFeature>();

    public static void SetCurrentAlertFeature(this HttpContext context, CurrentAlertFeature currentAlertFeature) =>
        context.Features.Set(currentAlertFeature);
}

public record CurrentPersonFeature(Guid PersonId, string FirstName, string MiddleName, string LastName)
{
    public string Name => (FirstName + " " + MiddleName).Trim() + " " + LastName;
}

public record CurrentMandatoryQualificationFeature(MandatoryQualification MandatoryQualification);

public record CurrentSupportTaskFeature(SupportTask SupportTask);

public record CurrentAlertFeature(Alert Alert);

public record CurrentProfessionalStatusFeature(RouteToProfessionalStatus RouteToProfessionalStatus);
