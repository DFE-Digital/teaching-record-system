using OneOf;
using Optional;
using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250327.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using V20240920Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using V20250203Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using V20250327Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20250327.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250327;

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
            Qts = source.Qts is { } qts ? new GetPersonResponseQts { Awarded = qts.HoldsFrom, StatusDescription = qts.StatusDescription, AwardedOrApprovedCount = qts.AwardedOrApprovedCount } : null,
            Eyts = source.Eyts is { } eyts ? new GetPersonResponseEyts { Awarded = eyts.HoldsFrom, StatusDescription = eyts.StatusDescription } : null,
            Induction = source.Induction.Map(ind => new V20250203Dtos.InductionInfo
            {
                Status = (V20250203Dtos.InductionStatus)(int)ind.Status,
                StartDate = ind.StartDate,
                CompletedDate = ind.CompletedDate
            }),
            InitialTeacherTraining = source.InitialTeacherTraining.Map(MapIttOneOf),
            NpqQualifications = Option.None<IReadOnlyCollection<GetPersonResponseNpqQualification>>(),
            MandatoryQualifications = source.MandatoryQualifications.Map(
                mqs => (IReadOnlyCollection<GetPersonResponseMandatoryQualification>)mqs
                    .Select(mq => new GetPersonResponseMandatoryQualification { Awarded = mq.EndDate, Specialism = mq.Specialism })
                    .AsReadOnly()),
            Sanctions = source.Sanctions.Map(
                ss => (IReadOnlyCollection<SanctionInfo>)ss.Select(MapSanctionInfo).AsReadOnly()),
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
            Qts = source.Qts is { } qts ? new V20250327Dtos.QtsInfo { Awarded = qts.HoldsFrom, StatusDescription = qts.StatusDescription, AwardedOrApprovedCount = qts.AwardedOrApprovedCount } : null,
            Eyts = source.Eyts is { } eyts ? new Core.ApiSchema.V3.V20240814.Dtos.EytsInfo { Awarded = eyts.HoldsFrom, StatusDescription = eyts.StatusDescription } : null,
            Alerts = source.Alerts.Select(MapAlert).AsReadOnly(),
            InductionStatus = (V20250203Dtos.InductionStatus)(int)source.Induction.Status,
            QtlsStatus = (V20250203Dtos.QtlsStatus)(int)source.QtlsStatus
        };

    private OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>> MapIttOneOf(
        OneOf<IReadOnlyCollection<GetPersonResultInitialTeacherTraining>, IReadOnlyCollection<GetPersonResultInitialTeacherTrainingForAppropriateBody>> source) =>
        source.Match(
            t0 => OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>>.FromT0(
                t0.Select(MapInitialTeacherTraining).AsReadOnly()),
            t1 => OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>>.FromT1(
                t1.Select(i => new GetPersonResponseInitialTeacherTrainingForAppropriateBody
                {
                    Provider = new GetPersonResponseInitialTeacherTrainingProvider { Name = i.Provider.Name, Ukprn = i.Provider.Ukprn }
                }).AsReadOnly()));

    private GetPersonResponseInitialTeacherTraining MapInitialTeacherTraining(GetPersonResultInitialTeacherTraining source) =>
        new()
        {
            Provider = source.Provider is { } p ? new GetPersonResponseInitialTeacherTrainingProvider { Name = p.Name, Ukprn = p.Ukprn } : null,
            Qualification = source.Qualification is { } q ? new GetPersonResponseInitialTeacherTrainingQualification { Name = q.Name } : null,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            ProgrammeType = null,
            ProgrammeTypeDescription = null,
            Result = null,
            AgeRange = source.AgeRange is { } ar ? new GetPersonResponseInitialTeacherTrainingAgeRange { Description = ar.Description } : null,
            Subjects = source.Subjects.Select(s => new GetPersonResponseInitialTeacherTrainingSubject { Code = s.Code, Name = s.Name }).AsReadOnly()
        };

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

    private SanctionInfo MapSanctionInfo(Implementation.Dtos.SanctionInfo source) =>
        new() { Code = source.Code, StartDate = source.StartDate };

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
            Qts = source.Qts is { } qts ? new V20250327Dtos.QtsInfo { Awarded = qts.HoldsFrom, StatusDescription = qts.StatusDescription, AwardedOrApprovedCount = qts.AwardedOrApprovedCount } : null,
            Eyts = source.Eyts is { } eyts ? new Core.ApiSchema.V3.V20240814.Dtos.EytsInfo { Awarded = eyts.HoldsFrom, StatusDescription = eyts.StatusDescription } : null,
            Alerts = source.Alerts.Select(MapAlert).AsReadOnly(),
            InductionStatus = (V20250203Dtos.InductionStatus)(int)source.Induction.Status,
            QtlsStatus = (V20250203Dtos.QtlsStatus)(int)source.QtlsStatus
        };
}
