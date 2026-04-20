using OneOf;
using Optional;
using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240920.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using V20240814Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;
using V20240920Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920;

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
            InitialTeacherTraining = source.InitialTeacherTraining.Map(MapIttOneOf),
            NpqQualifications = Option.None<IReadOnlyCollection<GetPersonResponseNpqQualification>>(),
            MandatoryQualifications = source.MandatoryQualifications.Map(
                mqs => (IReadOnlyCollection<GetPersonResponseMandatoryQualification>)mqs
                    .Select(mq => new GetPersonResponseMandatoryQualification { Awarded = mq.EndDate, Specialism = mq.Specialism })
                    .AsReadOnly()),
            HigherEducationQualifications = Option.None<IReadOnlyCollection<GetPersonResponseHigherEducationQualification>>(),
            Sanctions = source.Sanctions.Map(
                ss => (IReadOnlyCollection<SanctionInfo>)ss.Select(MapSanctionInfo).AsReadOnly()),
            Alerts = source.Alerts.Map(
                alerts => (IReadOnlyCollection<V20240920Dtos.Alert>)alerts.Select(MapAlert).AsReadOnly()),
            PreviousNames = source.PreviousNames.Map(
                pns => (IReadOnlyCollection<NameInfo>)pns.Select(MapNameInfo).AsReadOnly()),
            AllowIdSignInWithProhibitions = source.AllowIdSignInWithProhibitions
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
            InductionStatus = source.DqtInductionStatus is { } dqt ? new V20240814Dtos.DqtInductionStatusInfo { Status = MapDqtInductionStatus(dqt.Status), StatusDescription = dqt.StatusDescription } : null!,
            Qts = source.Qts is { } qts ? new V20240814Dtos.QtsInfo { Awarded = qts.HoldsFrom, StatusDescription = qts.StatusDescription } : null,
            Eyts = source.Eyts is { } eyts ? new V20240814Dtos.EytsInfo { Awarded = eyts.HoldsFrom, StatusDescription = eyts.StatusDescription } : null,
            Alerts = source.Alerts.Select(MapAlert).AsReadOnly()
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

    private GetPersonResponseInduction MapDqtInduction(GetPersonResultDqtInduction source) =>
        new()
        {
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Status = MapDqtInductionStatus(source.Status),
            StatusDescription = source.StatusDescription,
            CertificateUrl = source.CertificateUrl
        };

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

    internal V20240920Dtos.Alert MapAlert(Implementation.Dtos.Alert source) =>
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

    internal SanctionInfo MapSanctionInfo(Implementation.Dtos.SanctionInfo source) =>
        new() { Code = source.Code, StartDate = source.StartDate };

    internal NameInfo MapNameInfo(Implementation.Dtos.NameInfo source) =>
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
            InductionStatus = source.DqtInductionStatus is { } dqt ? new V20240814Dtos.DqtInductionStatusInfo { Status = MapDqtInductionStatus(dqt.Status), StatusDescription = dqt.StatusDescription } : null!,
            Qts = source.Qts is { } qts ? new V20240814Dtos.QtsInfo { Awarded = qts.HoldsFrom, StatusDescription = qts.StatusDescription } : null,
            Eyts = source.Eyts is { } eyts ? new V20240814Dtos.EytsInfo { Awarded = eyts.HoldsFrom, StatusDescription = eyts.StatusDescription } : null,
            Alerts = source.Alerts.Select(MapAlert).AsReadOnly()
        };

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
