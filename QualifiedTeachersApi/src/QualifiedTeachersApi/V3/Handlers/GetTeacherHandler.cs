using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.V3.ApiModels;
using QualifiedTeachersApi.V3.Requests;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Handlers;

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
                Contact.Fields.dfeta_QTSDate
            });

        if (teacher is null)
        {
            return null;
        }

        var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                dfeta_initialteachertraining.Fields.dfeta_Result,
                dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                dfeta_initialteachertraining.Fields.dfeta_TraineeID,
                dfeta_initialteachertraining.Fields.StateCode
            },
            establishmentColumnNames: new[]
            {
                Account.PrimaryIdAttribute,
                Account.Fields.dfeta_UKPRN,
                Account.Fields.Name
            },
            subjectColumnNames: new[]
            {
                dfeta_ittsubject.PrimaryIdAttribute,
                dfeta_ittsubject.Fields.dfeta_name,
                dfeta_ittsubject.Fields.dfeta_Value
            },
            qualificationColumnNames: new[]
            {
                dfeta_ittqualification.PrimaryIdAttribute,
                dfeta_ittqualification.Fields.dfeta_name
            },
            false);

        var qualifications = await _dataverseAdapter.GetQualificationsForTeacher(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                dfeta_qualification.Fields.dfeta_Type,
                dfeta_qualification.Fields.StateCode
            });

        return new GetTeacherResponse()
        {
            Trn = request.Trn,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            QtsDate = teacher.dfeta_QTSDate?.ToDateOnly(),
            InitialTeacherTraining = itt.Select(i => new GetTeacherResponseInitialTeacherTraining()
            {
                Qualification = MapIttQualification(i),
                ProgrammeType = i.dfeta_ProgrammeType?.ConvertToEnum<dfeta_ITTProgrammeType, IttProgrammeType>(),
                StartDate = i.dfeta_ProgrammeStartDate.ToDateOnly(),
                EndDate = i.dfeta_ProgrammeEndDate.ToDateOnly(),
                Result = i.dfeta_Result.HasValue ? i.dfeta_Result.Value.ConvertFromITTResult() : null,
                AgeRange = MapAgeRange(i.dfeta_AgeRangeFrom, i.dfeta_AgeRangeTo),
                Provider = MapProvider(i),
                Subjects = MapSubjects(i)
            }),
            NpqQualifications = MapNpqQualifications(qualifications)
        };
    }

    private static GetTeacherResponseInitialTeacherTrainingQualification MapIttQualification(dfeta_initialteachertraining initialTeacherTraining)
    {
        var qualification = initialTeacherTraining.Extract<dfeta_ittqualification>("qualification", dfeta_ittqualification.PrimaryIdAttribute);
        return
            qualification != null
            ? new GetTeacherResponseInitialTeacherTrainingQualification()
            {
                Name = qualification.dfeta_name
            }
            : null;
    }

    private static GetTeacherResponseInitialTeacherTrainingAgeRange MapAgeRange(dfeta_AgeRange? ageRangeFrom, dfeta_AgeRange? ageRangeTo)
    {
        var ageRangeDescription = new StringBuilder();
        string ageRangeFromName = ageRangeFrom.HasValue ? ageRangeFrom.Value.GetMetadata().Name : null;
        string ageRangeToName = ageRangeTo.HasValue ? ageRangeTo.Value.GetMetadata().Name : null;

        if (ageRangeFromName != null)
        {
            ageRangeDescription.AppendFormat("{0} ", ageRangeFromName);
        }

        if (ageRangeToName != null)
        {
            ageRangeDescription.AppendFormat("to {0} ", ageRangeToName);
        }

        if (ageRangeDescription.Length > 0)
        {
            ageRangeDescription.Append("years");
        }

        return
            ageRangeDescription.Length > 0
            ? new GetTeacherResponseInitialTeacherTrainingAgeRange()
            {
                Description = ageRangeDescription.ToString()
            }
            : null;
    }

    private static GetTeacherResponseInitialTeacherTrainingProvider MapProvider(dfeta_initialteachertraining initialTeacherTraining)
    {
        var establishment = initialTeacherTraining.Extract<Account>("establishment", Account.PrimaryIdAttribute);
        return
            establishment != null
            ? new GetTeacherResponseInitialTeacherTrainingProvider()
            {
                Name = establishment.Name,
                Ukprn = establishment.dfeta_UKPRN
            }
            : null;
    }

    private static IEnumerable<GetTeacherResponseInitialTeacherTrainingSubject> MapSubjects(dfeta_initialteachertraining initialTeacherTraining)
    {
        var subjects = new List<GetTeacherResponseInitialTeacherTrainingSubject>();
        for (var index = 1; index <= 3; index++)
        {
            var subject = initialTeacherTraining.Extract<dfeta_ittsubject>($"subject{index}", dfeta_ittsubject.PrimaryIdAttribute);
            if (subject != null)
            {
                subjects.Add(new GetTeacherResponseInitialTeacherTrainingSubject()
                {
                    Code = subject.dfeta_Value,
                    Name = subject.dfeta_name
                });
            }
        }

        return subjects;
    }

    private static IEnumerable<GetTeacherResponseNpqQualificationsQualification> MapNpqQualifications(dfeta_qualification[] qualifications)
    {
        var npqQualifications = new List<GetTeacherResponseNpqQualificationsQualification>();
        if (qualifications != null)
        {
            npqQualifications = qualifications
                .Where(q => q.dfeta_Type.HasValue
                    && q.dfeta_Type.Value.IsNpq()
                    && q.StateCode == dfeta_qualificationState.Active
                    && q.dfeta_CompletionorAwardDate.HasValue)
                .Select(q => new GetTeacherResponseNpqQualificationsQualification()
                {
                    Awarded = q.dfeta_CompletionorAwardDate.Value.ToDateOnly(),
                    Type = new GetTeacherResponseNpqQualificationsQualificationType()
                    {
                        Code = MapNpqQualificationType(q.dfeta_Type.Value).Value,
                        Name = q.dfeta_Type.Value.GetName(),
                    }
                })
                .ToList();
        }

        return npqQualifications;
    }

    private static GetTeacherResponseNpqQualificationsQualificationTypeCode? MapNpqQualificationType(dfeta_qualification_dfeta_Type qualificationType)
    {
        var mapped = qualificationType switch
        {
            dfeta_qualification_dfeta_Type.NPQEL => GetTeacherResponseNpqQualificationsQualificationTypeCode.NPQEL,
            dfeta_qualification_dfeta_Type.NPQEYL => GetTeacherResponseNpqQualificationsQualificationTypeCode.NPQEYL,
            dfeta_qualification_dfeta_Type.NPQH => GetTeacherResponseNpqQualificationsQualificationTypeCode.NPQH,
            dfeta_qualification_dfeta_Type.NPQLBC => GetTeacherResponseNpqQualificationsQualificationTypeCode.NPQLBC,
            dfeta_qualification_dfeta_Type.NPQLL => GetTeacherResponseNpqQualificationsQualificationTypeCode.NPQLL,
            dfeta_qualification_dfeta_Type.NPQLT => GetTeacherResponseNpqQualificationsQualificationTypeCode.NPQLT,
            dfeta_qualification_dfeta_Type.NPQLTD => GetTeacherResponseNpqQualificationsQualificationTypeCode.NPQLTD,
            dfeta_qualification_dfeta_Type.NPQML => GetTeacherResponseNpqQualificationsQualificationTypeCode.NPQML,
            dfeta_qualification_dfeta_Type.NPQSL => GetTeacherResponseNpqQualificationsQualificationTypeCode.NPQSL,
            _ => (GetTeacherResponseNpqQualificationsQualificationTypeCode?)null
        };

        return mapped;
    }
}
