using Microsoft.AspNetCore.Http.Features;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi;

public static class HttpContextExtensions
{
    public static CurrentPersonFeature GetCurrentPersonFeature(this HttpContext context) =>
        context.Features.GetRequiredFeature<CurrentPersonFeature>();

    public static void SetCurrentPersonFeature(this HttpContext context, CurrentPersonFeature currentPersonInfo) =>
        context.Features.Set(currentPersonInfo);

    public static void SetCurrentPersonFeature(this HttpContext context, ContactDetail contactDetail) =>
        SetCurrentPersonFeature(
            context,
            new CurrentPersonFeature(
                contactDetail.Contact.Id,
                contactDetail.Contact.FirstName,
                contactDetail.Contact.LastName));

    public static CurrentMandatoryQualificationFeature GetCurrentMandatoryQualificationFeature(this HttpContext context) =>
        context.Features.GetRequiredFeature<CurrentMandatoryQualificationFeature>();

    public static void SetCurrentMandatoryQualificationFeature(this HttpContext context, CurrentMandatoryQualificationFeature currentMandatoryQualificationFeature) =>
        context.Features.Set(currentMandatoryQualificationFeature);
}

public record CurrentPersonFeature(Guid PersonId, string FirstName, string LastName)
{
    public string Name => FirstName + " " + LastName;
}

public record CurrentMandatoryQualificationFeature(
    MandatoryQualification MandatoryQualification,
    MandatoryQualificationProvider? Provider,
    string? DqtEstablishmentName,
    string? DqtEstablishmentValue);
