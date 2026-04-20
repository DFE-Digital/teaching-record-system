using OneOf;
using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250627.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using V20240920Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using V20250203Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using V20250627Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250627;

[Mapper]
public partial class ApiMapper
{
    public GetPersonResponse MapGetPersonResponse(GetPersonResult source) =>
        new()
        {
            Trn = source.Trn,
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName,
            DateOfBirth = source.DateOfBirth,
            NationalInsuranceNumber = source.NationalInsuranceNumber,
            PendingNameChange = source.PendingNameChange,
            PendingDateOfBirthChange = source.PendingDateOfBirthChange,
            EmailAddress = source.EmailAddress,
            Qts = source.Qts is { } qts ? MapQtsInfo(qts) : null,
            Eyts = source.Eyts is { } eyts ? MapEytsInfo(eyts) : null,
            Induction = source.Induction.Map(MapInductionInfo),
            RoutesToProfessionalStatuses = source.RoutesToProfessionalStatuses.Map(MapRoutesOneOf),
            MandatoryQualifications = source.MandatoryQualifications.Map(
                mqs => (IReadOnlyCollection<GetPersonResponseMandatoryQualification>)mqs
                    .Select(mq => new GetPersonResponseMandatoryQualification
                    {
                        MandatoryQualificationId = mq.MandatoryQualificationId,
                        EndDate = mq.EndDate,
                        Specialism = mq.Specialism
                    }).AsReadOnly()),
            Alerts = source.Alerts.Map(
                alerts => (IReadOnlyCollection<V20240920Dtos.Alert>)alerts.Select(MapAlert).AsReadOnly()),
            PreviousNames = source.PreviousNames.Map(
                pns => (IReadOnlyCollection<NameInfo>)pns.Select(MapNameInfo).AsReadOnly()),
            AllowIdSignInWithProhibitions = source.AllowIdSignInWithProhibitions,
            QtlsStatus = (V20250203Dtos.QtlsStatus)(int)source.QtlsStatus
        };

    public FindPersonsResponse MapFindPersonsResponse(FindPersonsResult source) =>
        new()
        {
            Total = source.Total,
            Results = source.Items.Select(MapFindPersonsResponseResult).AsReadOnly()
        };

    public FindPersonsResponseResult MapFindPersonsResponseResult(FindPersonsResultItem source) =>
        new()
        {
            Trn = source.Trn,
            DateOfBirth = source.DateOfBirth,
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName,
            PreviousNames = source.PreviousNames.Select(MapNameInfo).AsReadOnly(),
            Qts = source.Qts is { } qts ? MapQtsInfo(qts) : null,
            Eyts = source.Eyts is { } eyts ? MapEytsInfo(eyts) : null,
            Alerts = source.Alerts.Select(MapAlert).AsReadOnly(),
            Induction = source.Induction is { } ind ? MapInductionInfo(ind) : null,
            QtlsStatus = (V20250203Dtos.QtlsStatus)(int)source.QtlsStatus
        };

    public Core.Models.RouteToProfessionalStatusStatus MapRouteToProfessionalStatusStatus(
        V20250627Dtos.RouteToProfessionalStatusStatus source) =>
        (Core.Models.RouteToProfessionalStatusStatus)(int)source;

    private OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>> MapRoutesOneOf(
        OneOf<IReadOnlyCollection<GetPersonResultRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResultRouteToProfessionalStatusForAppropriateBody>> source) =>
        source.Match(
            t0 => OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>.FromT0(
                t0.Select(MapRouteToProfessionalStatus).AsReadOnly()),
            t1 => OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>.FromT1(
                t1.Select(i => new GetPersonResponseRouteToProfessionalStatusForAppropriateBody
                {
                    TrainingProvider = MapTrainingProvider(i.TrainingProvider)
                }).AsReadOnly()));

    private GetPersonResponseRouteToProfessionalStatus MapRouteToProfessionalStatus(GetPersonResultRouteToProfessionalStatus source) =>
        new()
        {
            RouteToProfessionalStatusId = source.RouteToProfessionalStatusId,
            RouteToProfessionalStatusType = MapRouteToProfessionalStatusType(source.RouteToProfessionalStatusType),
            Status = (V20250627Dtos.RouteToProfessionalStatusStatus)(int)source.Status,
            HoldsFrom = source.HoldsFrom,
            TrainingStartDate = source.TrainingStartDate,
            TrainingEndDate = source.TrainingEndDate,
            TrainingSubjects = source.TrainingSubjects.Select(MapTrainingSubject).AsReadOnly(),
            TrainingAgeSpecialism = source.TrainingAgeSpecialism is { } ta ? MapTrainingAgeSpecialism(ta) : null,
            TrainingCountry = source.TrainingCountry is { } tc ? new V20250627Dtos.TrainingCountry { Reference = tc.Reference, Name = tc.Name } : null,
            TrainingProvider = source.TrainingProvider is { } tp ? MapTrainingProvider(tp) : null,
            DegreeType = source.DegreeType is { } dt ? new V20250627Dtos.DegreeType { DegreeTypeId = dt.DegreeTypeId, Name = dt.Name } : null,
            InductionExemption = new GetPersonResponseProfessionalStatusInductionExemption
            {
                IsExempt = source.InductionExemption.IsExempt,
                ExemptionReasons = source.InductionExemption.ExemptionReasons
                    .Select(MapInductionExemptionReason)
                    .AsReadOnly()
            }
        };

    private V20250627Dtos.InductionInfo MapInductionInfo(Implementation.Dtos.InductionInfo source) =>
        new()
        {
            Status = (V20250203Dtos.InductionStatus)(int)source.Status,
            StartDate = source.StartDate,
            CompletedDate = source.CompletedDate,
            ExemptionReasons = source.ExemptionReasons.Select(MapInductionExemptionReason).AsReadOnly()
        };

    private V20250627Dtos.QtsInfo MapQtsInfo(Implementation.Dtos.QtsInfo source) =>
        new()
        {
            HoldsFrom = source.HoldsFrom,
            Routes = source.Routes.Select(r => new V20250627Dtos.QtsInfoRoute
            {
                RouteToProfessionalStatusType = MapRouteToProfessionalStatusType(r.RouteToProfessionalStatusType)
            }).AsReadOnly()
        };

    private V20250627Dtos.EytsInfo MapEytsInfo(Implementation.Dtos.EytsInfo source) =>
        new()
        {
            HoldsFrom = source.HoldsFrom,
            Routes = source.Routes.Select(r => new V20250627Dtos.EytsInfoRoute
            {
                RouteToProfessionalStatusType = MapRouteToProfessionalStatusType(r.RouteToProfessionalStatusType)
            }).AsReadOnly()
        };

    private V20250627Dtos.RouteToProfessionalStatusType MapRouteToProfessionalStatusType(PostgresModels.RouteToProfessionalStatusType source) =>
        new()
        {
            RouteToProfessionalStatusTypeId = source.RouteToProfessionalStatusTypeId,
            Name = source.Name,
            ProfessionalStatusType = (V20250627Dtos.ProfessionalStatusType)(int)source.ProfessionalStatusType
        };

    private V20250627Dtos.TrainingSubject MapTrainingSubject(PostgresModels.TrainingSubject source) =>
        new() { Reference = source.Reference, Name = source.Name };

    private V20250627Dtos.TrainingProvider MapTrainingProvider(PostgresModels.TrainingProvider source) =>
        new() { Ukprn = source.Ukprn!, Name = source.Name };

    private V20250627Dtos.TrainingAgeSpecialism MapTrainingAgeSpecialism(Implementation.Dtos.TrainingAgeSpecialism source) =>
        new()
        {
            Type = (Core.ApiSchema.V3.V20250425.Dtos.TrainingAgeSpecialismType)(int)source.Type!.Value,
            From = source.From,
            To = source.To
        };

    private V20250627Dtos.InductionExemptionReason MapInductionExemptionReason(PostgresModels.InductionExemptionReason source) =>
        new() { InductionExemptionReasonId = source.InductionExemptionReasonId, Name = source.Name };

    private V20240920Dtos.Alert MapAlert(Implementation.Dtos.Alert source) =>
        new()
        {
            AlertId = source.AlertId,
            AlertType = new V20240920Dtos.AlertType
            {
                AlertTypeId = source.AlertType.AlertTypeId,
                Name = source.AlertType.Name,
                AlertCategory = new V20240920Dtos.AlertCategory
                {
                    AlertCategoryId = source.AlertType.AlertCategory.AlertCategoryId,
                    Name = source.AlertType.AlertCategory.Name
                }
            },
            Details = source.Details,
            StartDate = source.StartDate,
            EndDate = source.EndDate
        };

    private NameInfo MapNameInfo(Implementation.Dtos.NameInfo source) =>
        new() { FirstName = source.FirstName, MiddleName = source.MiddleName, LastName = source.LastName };

    public FindPersonResponseResult MapFindPersonResponseResult(FindPersonsResultItem source) =>
        new()
        {
            Trn = source.Trn,
            DateOfBirth = source.DateOfBirth,
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName,
            PreviousNames = source.PreviousNames.Select(MapNameInfo).AsReadOnly(),
            Qts = source.Qts is { } qts ? MapQtsInfo(qts) : null,
            Eyts = source.Eyts is { } eyts ? MapEytsInfo(eyts) : null,
            Alerts = source.Alerts.Select(MapAlert).AsReadOnly(),
            Induction = source.Induction is { } ind ? MapInductionInfo(ind) : null,
            QtlsStatus = (V20250203Dtos.QtlsStatus)(int)source.QtlsStatus
        };
}
