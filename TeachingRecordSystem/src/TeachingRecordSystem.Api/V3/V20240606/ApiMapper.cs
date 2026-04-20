using Optional;
using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240606.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using V20240606Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240606;

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
            Qts = source.Qts is { } qts ? new GetPersonResponseQts { Awarded = qts.HoldsFrom, CertificateUrl = qts.CertificateUrl, StatusDescription = qts.StatusDescription } : null,
            Eyts = source.Eyts is { } eyts ? new GetPersonResponseEyts { Awarded = eyts.HoldsFrom, CertificateUrl = eyts.CertificateUrl, StatusDescription = eyts.StatusDescription } : null,
            Induction = source.DqtInduction.Map(d => d is null ? (GetPersonResponseInduction?)null : MapDqtInduction(d)),
            InitialTeacherTraining = source.InitialTeacherTraining.Map(
                itt => (IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>)itt.AsT0
                    .Select(MapInitialTeacherTraining).AsReadOnly()),
            NpqQualifications = Option.None<IReadOnlyCollection<GetPersonResponseNpqQualification>>(),
            MandatoryQualifications = source.MandatoryQualifications.Map(
                mqs => (IReadOnlyCollection<GetPersonResponseMandatoryQualification>)mqs
                    .Select(mq => new GetPersonResponseMandatoryQualification { Awarded = mq.EndDate, Specialism = mq.Specialism })
                    .AsReadOnly()),
            HigherEducationQualifications = Option.None<IReadOnlyCollection<GetPersonResponseHigherEducationQualification>>(),
            Sanctions = source.Sanctions.Map(
                ss => (IReadOnlyCollection<SanctionInfo>)ss.Select(MapSanctionInfo).AsReadOnly()),
            Alerts = source.Alerts.Map(
                alerts => (IReadOnlyCollection<AlertInfo>)alerts.Select(MapAlertInfo).AsReadOnly()),
            PreviousNames = source.PreviousNames.Map(
                pns => (IReadOnlyCollection<NameInfo>)pns.Select(MapNameInfo).AsReadOnly()),
            AllowIdSignInWithProhibitions = source.AllowIdSignInWithProhibitions
        };

    public FindPersonResponseResult MapFindPersonResponseResult(FindPersonsResultItem source) =>
        new()
        {
            Trn = source.Trn,
            DateOfBirth = source.DateOfBirth,
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName,
            Sanctions = source.Sanctions.Select(MapSanctionInfo).AsReadOnly(),
            PreviousNames = source.PreviousNames.Select(MapNameInfo).AsReadOnly()
        };

    public V20240606Dtos.TrnRequestInfo MapTrnRequestInfo(Implementation.Dtos.TrnRequestInfo source) =>
        new()
        {
            RequestId = source.RequestId,
            Status = (V20240606Dtos.TrnRequestStatus)(int)source.Status,
            Trn = source.Trn
        };

    public CreateNameChangeResponse MapCreateNameChangeResponse(CreateNameChangeRequestResult source) =>
        new() { CaseNumber = source.CaseNumber };

    public CreateDateOfBirthChangeResponse MapCreateDateOfBirthChangeResponse(CreateDateOfBirthChangeRequestResult source) =>
        new() { CaseNumber = source.CaseNumber };

    private GetPersonResponseInduction MapDqtInduction(GetPersonResultDqtInduction source) =>
        new()
        {
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Status = MapDqtInductionStatus(source.Status),
            StatusDescription = source.StatusDescription,
            CertificateUrl = source.CertificateUrl,
            Periods = source.Periods.Select(p => new GetPersonResponseInductionPeriod
            {
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Terms = p.Terms,
                AppropriateBody = p.AppropriateBody is { } ab
                    ? new GetPersonResponseInductionPeriodAppropriateBody { Name = ab.Name }
                    : null
            }).AsReadOnly()
        };

    private GetPersonResponseInitialTeacherTraining MapInitialTeacherTraining(GetPersonResultInitialTeacherTraining source) =>
        new()
        {
            Qualification = source.Qualification is { } q ? new GetPersonResponseInitialTeacherTrainingQualification { Name = q.Name } : null,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            ProgrammeType = null,
            ProgrammeTypeDescription = null,
            Result = null,
            AgeRange = source.AgeRange is { } ar ? new GetPersonResponseInitialTeacherTrainingAgeRange { Description = ar.Description } : null,
            Provider = source.Provider is { } p ? new GetPersonResponseInitialTeacherTrainingProvider { Name = p.Name, Ukprn = p.Ukprn } : null,
            Subjects = source.Subjects.Select(s => new GetPersonResponseInitialTeacherTrainingSubject { Code = s.Code, Name = s.Name }).AsReadOnly()
        };

    private SanctionInfo MapSanctionInfo(Implementation.Dtos.SanctionInfo source) =>
        new() { Code = source.Code, StartDate = source.StartDate };

    private AlertInfo MapAlertInfo(Implementation.Dtos.Alert source) =>
        new()
        {
            AlertType = AlertType.Prohibition,
            DqtSanctionCode = source.AlertType.DqtSanctionCode!,
            StartDate = source.StartDate,
            EndDate = source.EndDate
        };

    private NameInfo MapNameInfo(Implementation.Dtos.NameInfo source) =>
        new() { FirstName = source.FirstName, MiddleName = source.MiddleName, LastName = source.LastName };

    private static Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus MapDqtInductionStatus(Implementation.Dtos.DqtInductionStatus source) =>
        source switch
        {
            Implementation.Dtos.DqtInductionStatus.Exempt => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.Exempt,
            Implementation.Dtos.DqtInductionStatus.Fail => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.Fail,
            Implementation.Dtos.DqtInductionStatus.FailedInWales => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.FailedinWales,
            Implementation.Dtos.DqtInductionStatus.InductionExtended => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.InductionExtended,
            Implementation.Dtos.DqtInductionStatus.InProgress => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.InProgress,
            Implementation.Dtos.DqtInductionStatus.NotYetCompleted => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.NotYetCompleted,
            Implementation.Dtos.DqtInductionStatus.Pass => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.Pass,
            Implementation.Dtos.DqtInductionStatus.PassedInWales => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.PassedinWales,
            Implementation.Dtos.DqtInductionStatus.RequiredToComplete => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.RequiredtoComplete,
            _ => throw new ArgumentOutOfRangeException(nameof(source))
        };
}
