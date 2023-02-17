using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Crm.Sdk.Messages;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.V2.ApiModels;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;
using QualifiedTeachersApi.Validation;

namespace QualifiedTeachersApi.V2.Handlers
{
    public class GetTeacherHandler : IRequestHandler<GetTeacherRequest, GetTeacherResponse>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public GetTeacherHandler(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<GetTeacherResponse> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
        {
            var teachers = await _dataverseAdapter.GetTeachersByTrn(
                request.Trn,
                columnNames: new[]
                {
                    Contact.Fields.FirstName,
                    Contact.Fields.LastName,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_EYTSDate,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_ActiveSanctions,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.dfeta_TRN
                },
                activeOnly: true);

            if (teachers.Length == 0)
            {
                return null;
            }
            else if (teachers.Length > 1)
            {
                throw new ErrorException(ErrorRegistry.MultipleTeachersFoundWithSpecifiedTrn());
            }

            var teacher = teachers[0];

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
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId
                },
                establishmentColumnNames: new[]
                {
                    Account.PrimaryIdAttribute,
                    Account.Fields.dfeta_UKPRN
                });

            return new GetTeacherResponse()
            {
                DateOfBirth = teacher.BirthDate.ToDateOnly(),
                FirstName = teacher.FirstName,
                HasActiveSanctions = teacher.dfeta_ActiveSanctions == true,
                LastName = teacher.LastName,
                NationalInsuranceNumber = teacher.dfeta_NINumber,
                Trn = teacher.dfeta_TRN,
                QtsDate = teacher.dfeta_QTSDate.ToDateOnly(),
                EytsDate = teacher.dfeta_EYTSDate.ToDateOnly(),
                EarlyYearsStatus = earlyYearsStatus is not null ?
                    new GetTeacherResponseEarlyYearsStatus()
                    {
                        Name = earlyYearsStatus.dfeta_name,
                        Value = earlyYearsStatus.dfeta_Value
                    } :
                    null,
                InitialTeacherTraining = itt.Select(i => new GetTeacherResponseInitialTeacherTraining()
                {
                    ProgrammeEndDate = i.dfeta_ProgrammeEndDate.ToDateOnly(),
                    ProgrammeStartDate = i.dfeta_ProgrammeStartDate.ToDateOnly(),
                    ProgrammeType = i.dfeta_ProgrammeType?.ConvertToEnum<dfeta_ITTProgrammeType, IttProgrammeType>(),
                    Result = i.dfeta_Result.HasValue ? i.dfeta_Result.Value.ConvertFromITTResult() : null,
                    Provider = new()
                    {
                        Ukprn = i.Extract<Account>("establishment", Account.PrimaryIdAttribute).dfeta_UKPRN
                    }
                })
            };
        }
    }
}
