#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetTeacherHandler : IRequestHandler<GetTeacherRequest, GetTeacherResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;

    public GetTeacherHandler(IDataverseAdapter dataverseAdapter)
    {
        _dataverseAdapter = dataverseAdapter;
    }

    public async Task<GetTeacherResponse> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dataverseAdapter.GetTeacherByTrn(
            request.Trn,
            columnNames: new[]
            {
                Contact.Fields.FirstName,
                Contact.Fields.LastName,
                Contact.Fields.MiddleName,
                Contact.Fields.BirthDate,
                Contact.Fields.dfeta_EYTSDate,
                Contact.Fields.dfeta_QTSDate,
                Contact.Fields.dfeta_ActiveSanctions,
                Contact.Fields.dfeta_NINumber,
                Contact.Fields.dfeta_TRN,
                Contact.Fields.dfeta_HUSID
            },
            activeOnly: true);

        if (teacher is null)
        {
            return null;
        }

        var qtsRegistrations = await _dataverseAdapter.GetQtsRegistrationsByTeacher(
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
            earlyYearsStatus = await _dataverseAdapter.GetEarlyYearsStatus(earlyYearsQtsRegistration.dfeta_EarlyYearsStatusId.Id);
        }

        var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
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

        return new GetTeacherResponse()
        {
            DateOfBirth = teacher.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            FirstName = teacher.FirstName,
            HasActiveSanctions = teacher.dfeta_ActiveSanctions == true,
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
            InitialTeacherTraining = itt.Select(i => new GetTeacherResponseInitialTeacherTraining()
            {
                ProgrammeEndDate = i.dfeta_ProgrammeEndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                ProgrammeStartDate = i.dfeta_ProgrammeStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                ProgrammeType = i.dfeta_ProgrammeType?.ConvertToEnum<dfeta_ITTProgrammeType, IttProgrammeType>(),
                Result = i.dfeta_Result.HasValue ? i.dfeta_Result.Value.ConvertFromITTResult() : null,
                Provider = new()
                {
                    Ukprn = i.Extract<Account>("establishment", Account.PrimaryIdAttribute).dfeta_UKPRN
                },
                HusId = i.dfeta_TraineeID,
                Active = i.StateCode == dfeta_initialteachertrainingState.Active
            })
        };
    }
}
