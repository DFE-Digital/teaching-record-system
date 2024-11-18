#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetTeacherHandler : IRequestHandler<GetTeacherRequest, GetTeacherResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly TrsDbContext _dbContext;

    public GetTeacherHandler(IDataverseAdapter dataverseAdapter, TrsDbContext dbContext)
    {
        _dataverseAdapter = dataverseAdapter;
        _dbContext = dbContext;
    }

    public async Task<GetTeacherResponse> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dataverseAdapter.GetTeacherByTrnAsync(
            request.Trn,
            columnNames: new[]
            {
                Contact.Fields.FirstName,
                Contact.Fields.LastName,
                Contact.Fields.MiddleName,
                Contact.Fields.BirthDate,
                Contact.Fields.dfeta_EYTSDate,
                Contact.Fields.dfeta_QTSDate,
                Contact.Fields.dfeta_NINumber,
                Contact.Fields.dfeta_TRN,
                Contact.Fields.dfeta_HUSID,
                Contact.Fields.dfeta_AllowPiiUpdatesFromRegister
            },
            activeOnly: true);

        if (teacher is null)
        {
            return null;
        }

        var qtsRegistrations = await _dataverseAdapter.GetQtsRegistrationsByTeacherAsync(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_qtsregistration.Fields.dfeta_QTSDate,
                dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId
            });

        dfeta_earlyyearsstatus earlyYearsStatus = null;
        var earlyYearsQtsRegistration = qtsRegistrations.SingleOrDefault(qts => qts.dfeta_EarlyYearsStatusId is not null);
        if (earlyYearsQtsRegistration is not null)
        {
            earlyYearsStatus = await _dataverseAdapter.GetEarlyYearsStatusAsync(earlyYearsQtsRegistration.dfeta_EarlyYearsStatusId.Id);
        }

        var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacherAsync(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                dfeta_initialteachertraining.Fields.dfeta_Result,
                dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                dfeta_initialteachertraining.Fields.dfeta_TraineeID,
                dfeta_initialteachertraining.Fields.StateCode
            },
            establishmentColumnNames: new[]
            {
                Account.PrimaryIdAttribute,
                Account.Fields.dfeta_UKPRN
            },
            null,
            null,
            request.IncludeInactive != true);

        var hasActiveAlert = await _dbContext.Alerts.Where(a => a.PersonId == teacher.Id && a.IsOpen).AnyAsync();

        return new GetTeacherResponse()
        {
            DateOfBirth = teacher.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            FirstName = teacher.FirstName,
            HasActiveSanctions = hasActiveAlert,
            LastName = teacher.LastName,
            MiddleName = teacher.MiddleName,
            NationalInsuranceNumber = teacher.dfeta_NINumber,
            Trn = teacher.dfeta_TRN,
            QtsDate = teacher.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EytsDate = teacher.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            HusId = teacher.dfeta_HUSID,
            EarlyYearsStatus = earlyYearsStatus is not null ?
                new GetTeacherResponseEarlyYearsStatus()
                {
                    Name = earlyYearsStatus.dfeta_name,
                    Value = earlyYearsStatus.dfeta_Value
                } :
                null,
            InitialTeacherTraining = itt.Select(i =>
            {
                var provider = i.Extract<Account>("establishment", Account.PrimaryIdAttribute);

                return new GetTeacherResponseInitialTeacherTraining()
                {
                    ProgrammeEndDate = i.dfeta_ProgrammeEndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    ProgrammeStartDate = i.dfeta_ProgrammeStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    ProgrammeType = i.dfeta_ProgrammeType?.ConvertToEnumByValue<dfeta_ITTProgrammeType, IttProgrammeType>(),
                    Result = i.dfeta_Result.HasValue ? i.dfeta_Result.Value.ConvertFromITTResult() : null,
                    Provider = provider is not null ?
                        new()
                        {
                            Ukprn = provider.dfeta_UKPRN
                        } :
                        null,
                    HusId = i.dfeta_TraineeID,
                    Active = i.StateCode == dfeta_initialteachertrainingState.Active
                };
            }),
            AllowPIIUpdates = teacher.dfeta_AllowPiiUpdatesFromRegister ?? false
        };
    }
}
