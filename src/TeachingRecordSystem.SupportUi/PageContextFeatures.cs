using Microsoft.AspNetCore.Http.Features;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi;

public static class HttpContextExtensions
{
    extension(HttpContext context)
    {
        public CurrentPersonFeature GetCurrentPersonFeature() =>
            context.Features.GetRequiredFeature<CurrentPersonFeature>();

        public void SetCurrentPersonFeature(CurrentPersonFeature currentPersonInfo) =>
            context.Features.Set(currentPersonInfo);

        public void SetCurrentPersonFeature(Person person) =>
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

        public CurrentOneLoginUserFeature GetCurrentOneLoginUserFeature() =>
            context.Features.GetRequiredFeature<CurrentOneLoginUserFeature>();

        public void SetCurrentOneLoginUserFeature(CurrentOneLoginUserFeature currentOneLoginUserFeature) =>
            context.Features.Set(currentOneLoginUserFeature);

        public void SetCurrentOneLoginUserFeature(OneLoginUser oneLoginUser) =>
            SetCurrentOneLoginUserFeature(
                context,
                new CurrentOneLoginUserFeature(
                    oneLoginUser.Subject,
                    oneLoginUser.EmailAddress,
                    oneLoginUser.PersonId,
                    oneLoginUser.VerifiedOn,
                    oneLoginUser.VerifiedNames,
                    oneLoginUser.VerifiedDatesOfBirth));

        public CurrentMandatoryQualificationFeature GetCurrentMandatoryQualificationFeature() =>
            context.Features.GetRequiredFeature<CurrentMandatoryQualificationFeature>();

        public void SetCurrentMandatoryQualificationFeature(CurrentMandatoryQualificationFeature currentMandatoryQualificationFeature) =>
            context.Features.Set(currentMandatoryQualificationFeature);

        public CurrentProfessionalStatusFeature GetCurrentProfessionalStatusFeature() =>
            context.Features.GetRequiredFeature<CurrentProfessionalStatusFeature>();

        public void SetCurrentProfessionalStatusFeature(CurrentProfessionalStatusFeature currentProfessionalStatusFeature) =>
            context.Features.Set(currentProfessionalStatusFeature);

        public CurrentSupportTaskFeature GetCurrentSupportTaskFeature() =>
            context.Features.GetRequiredFeature<CurrentSupportTaskFeature>();

        public void SetCurrentSupportTaskFeature(CurrentSupportTaskFeature currentSupportTaskFeature) =>
            context.Features.Set(currentSupportTaskFeature);

        public CurrentAlertFeature GetCurrentAlertFeature() =>
            context.Features.GetRequiredFeature<CurrentAlertFeature>();

        public void SetCurrentAlertFeature(CurrentAlertFeature currentAlertFeature) =>
            context.Features.Set(currentAlertFeature);
    }
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
