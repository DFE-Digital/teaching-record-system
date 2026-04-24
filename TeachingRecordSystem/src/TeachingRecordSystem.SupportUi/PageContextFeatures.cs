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
                person.Trn,
                person.Status,
                person.FirstName,
                person.MiddleName,
                person.LastName,
                person.EmailAddress,
                person.DateOfBirth,
                person.NationalInsuranceNumber));

    public static CurrentOneLoginUserFeature GetCurrentOneLoginUserFeature(this HttpContext context) =>
        context.Features.GetRequiredFeature<CurrentOneLoginUserFeature>();

    public static void SetCurrentOneLoginUserFeature(this HttpContext context, CurrentOneLoginUserFeature currentOneLoginUserFeature) =>
        context.Features.Set(currentOneLoginUserFeature);

    public static void SetCurrentOneLoginUserFeature(this HttpContext context, OneLoginUser oneLoginUser) =>
        SetCurrentOneLoginUserFeature(
            context,
            new CurrentOneLoginUserFeature(
                oneLoginUser.Subject,
                oneLoginUser.EmailAddress,
                oneLoginUser.PersonId,
                oneLoginUser.VerifiedOn,
                oneLoginUser.VerifiedNames,
                oneLoginUser.VerifiedDatesOfBirth));

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

public record CurrentPersonFeature(
    Guid PersonId,
    string Trn,
    PersonStatus Status,
    string FirstName,
    string MiddleName,
    string LastName,
    string? EmailAddress,
    DateOnly? DateOfBirth,
    string? NationalInsuranceNumber)
{
    public string Name => (FirstName + " " + MiddleName).Trim() + " " + LastName;
}

public record CurrentOneLoginUserFeature(
    string Subject,
    string? EmailAddress,
    Guid? PersonId,
    DateTime? VerifiedOn,
    string[][]? VerifiedNames,
    DateOnly[]? VerifiedDatesOfBirth);

public record CurrentMandatoryQualificationFeature(MandatoryQualification MandatoryQualification);

public record CurrentSupportTaskFeature(SupportTask SupportTask);

public record CurrentAlertFeature(Alert Alert);

public record CurrentProfessionalStatusFeature(RouteToProfessionalStatus RouteToProfessionalStatus);
