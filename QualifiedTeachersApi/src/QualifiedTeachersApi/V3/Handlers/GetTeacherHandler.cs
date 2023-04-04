using System;
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
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.BirthDate,
                Contact.Fields.dfeta_NINumber,
                Contact.Fields.dfeta_QTSDate,
                Contact.Fields.dfeta_EYTSDate
            });

        if (teacher is null)
        {
            return null;
        }

        var (induction, inductionPeriods) = await _dataverseAdapter.GetInductionByTeacher(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_induction.PrimaryIdAttribute,
                dfeta_induction.Fields.dfeta_StartDate,
                dfeta_induction.Fields.dfeta_CompletionDate,
                dfeta_induction.Fields.dfeta_InductionStatus
            },
            inductionPeriodColumnNames: new[]
            {
                dfeta_inductionperiod.Fields.dfeta_InductionId,
                dfeta_inductionperiod.Fields.dfeta_StartDate,
                dfeta_inductionperiod.Fields.dfeta_EndDate,
                dfeta_inductionperiod.Fields.dfeta_Numberofterms,
                dfeta_inductionperiod.Fields.dfeta_AppropriateBodyId
            },
            appropriateBodyColumnNames: new[]
            {
                Account.PrimaryIdAttribute,
                Account.Fields.Name
            });

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
                dfeta_qualification.Fields.StateCode,
                dfeta_qualification.Fields.dfeta_MQ_Date,
                dfeta_qualification.Fields.dfeta_MQ_SpecialismId
            },
            specialismColumnNames: new[]
            {
                dfeta_specialism.PrimaryIdAttribute,
                dfeta_specialism.Fields.dfeta_name
            });

        var nameChangeSubject = await _dataverseAdapter.GetSubjectByTitle("Change of Name", columnNames: Array.Empty<string>());
        var dateOfBirthChangeSubject = await _dataverseAdapter.GetSubjectByTitle("Change of Date of Birth", columnNames: Array.Empty<string>());

        var incidents = await _dataverseAdapter.GetIncidentsByContactId(
            teacher.Id,
            IncidentState.Active,
            columnNames: new[] { Incident.Fields.SubjectId, Incident.Fields.StateCode });

        var pendingNameChange = incidents.Any(i => i.SubjectId.Id == nameChangeSubject.Id);
        var pendingDateOfBirthChange = incidents.Any(i => i.SubjectId.Id == dateOfBirthChangeSubject.Id);

        return new GetTeacherResponse()
        {
            Trn = request.Trn,
            FirstName = teacher.FirstName,
            MiddleName = teacher.MiddleName,
            LastName = teacher.LastName,
            DateOfBirth = teacher.BirthDate.Value.ToDateOnly(),
            NationalInsuranceNumber = teacher.dfeta_NINumber,
            PendingNameChange = pendingNameChange,
            PendingDateOfBirthChange = pendingDateOfBirthChange,
            Qts = MapQts(teacher.dfeta_QTSDate?.ToDateOnly()),
            Eyts = MapEyts(teacher.dfeta_EYTSDate?.ToDateOnly()),
            Induction = MapInduction(induction, inductionPeriods),
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
            NpqQualifications = MapNpqQualifications(qualifications),
            MandatoryQualifications = MapMandatoryQualifications(qualifications)
        };
    }

    private static GetTeacherResponseQts MapQts(DateOnly? qtsDate)
    {
        return
            qtsDate != null
            ? new GetTeacherResponseQts()
            {
                Awarded = qtsDate.Value,
                CertificateUrl = "/v3/certificates/qts"
            }
            : null;
    }

    private static GetTeacherResponseEyts MapEyts(DateOnly? eytsDate)
    {
        return
            eytsDate != null
            ? new GetTeacherResponseEyts()
            {
                Awarded = eytsDate.Value,
                CertificateUrl = "/v3/certificates/eyts"
            }
            : null;
    }

    private static GetTeacherResponseInduction MapInduction(dfeta_induction induction, dfeta_inductionperiod[] inductionperiods)
    {
        return
            induction != null
            ? new GetTeacherResponseInduction()
            {
                StartDate = induction.dfeta_StartDate.ToDateOnly(),
                EndDate = induction.dfeta_CompletionDate.ToDateOnly(),
                Status = MapInductionStatus(induction.dfeta_InductionStatus),
                CertificateUrl = induction.dfeta_InductionStatus == dfeta_InductionStatus.Pass || induction.dfeta_InductionStatus == dfeta_InductionStatus.PassedinWales
                ? "/v3/certificates/induction"
                : null,
                Periods = inductionperiods.Select(p => MapInductionPeriod(p)).ToArray()
            }
            : null;
    }

    private static GetTeacherResponseInductionPeriod MapInductionPeriod(dfeta_inductionperiod inductionPeriod)
    {
        var appropriateBody = inductionPeriod.Extract<Account>("appropriatebody", Account.PrimaryIdAttribute);
        return new GetTeacherResponseInductionPeriod()
        {
            StartDate = inductionPeriod.dfeta_StartDate.ToDateOnly(),
            EndDate = inductionPeriod.dfeta_EndDate.ToDateOnly(),
            Terms = inductionPeriod.dfeta_Numberofterms,
            AppropriateBody = new GetTeacherResponseInductionPeriodAppropriateBody()
            {
                Name = appropriateBody.Name
            }
        };
    }

    private static InductionStatus? MapInductionStatus(dfeta_InductionStatus? inductionStatus)
    {
        var mapped = inductionStatus switch
        {
            dfeta_InductionStatus.Exempt => InductionStatus.Exempt,
            dfeta_InductionStatus.Fail => InductionStatus.Fail,
            dfeta_InductionStatus.FailedinWales => InductionStatus.FailedinWales,
            dfeta_InductionStatus.InductionExtended => InductionStatus.InductionExtended,
            dfeta_InductionStatus.InProgress => InductionStatus.InProgress,
            dfeta_InductionStatus.NotYetCompleted => InductionStatus.NotYetCompleted,
            dfeta_InductionStatus.Pass => InductionStatus.Pass,
            dfeta_InductionStatus.PassedinWales => InductionStatus.PassedinWales,
            dfeta_InductionStatus.RequiredtoComplete => InductionStatus.RequiredtoComplete,
            null => (InductionStatus?)null,
            _ => throw new NotImplementedException($"{nameof(InductionStatus)}: {inductionStatus} is not currently supported.")
        };

        return mapped;
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
        return
            qualifications
                ?.Where(q => q.dfeta_Type.HasValue
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
                    },
                    CertificateUrl = $"/v3/certificates/npq/{q.Id}"
                })
                .ToArray() ?? Array.Empty<GetTeacherResponseNpqQualificationsQualification>();
    }

    private static NpqQualificationType? MapNpqQualificationType(dfeta_qualification_dfeta_Type qualificationType)
    {
        var mapped = qualificationType switch
        {
            dfeta_qualification_dfeta_Type.NPQEL => NpqQualificationType.NPQEL,
            dfeta_qualification_dfeta_Type.NPQEYL => NpqQualificationType.NPQEYL,
            dfeta_qualification_dfeta_Type.NPQH => NpqQualificationType.NPQH,
            dfeta_qualification_dfeta_Type.NPQLBC => NpqQualificationType.NPQLBC,
            dfeta_qualification_dfeta_Type.NPQLL => NpqQualificationType.NPQLL,
            dfeta_qualification_dfeta_Type.NPQLT => NpqQualificationType.NPQLT,
            dfeta_qualification_dfeta_Type.NPQLTD => NpqQualificationType.NPQLTD,
            dfeta_qualification_dfeta_Type.NPQML => NpqQualificationType.NPQML,
            dfeta_qualification_dfeta_Type.NPQSL => NpqQualificationType.NPQSL,
            _ => throw new NotImplementedException($"Qualification Type {qualificationType} is not currently supported.")
        };

        return mapped;
    }

    private static IEnumerable<GetTeacherResponseMandatoryQualificationsQualification> MapMandatoryQualifications(dfeta_qualification[] qualifications)
    {
        return
            qualifications
                ?.Where(q => q.dfeta_Type.HasValue
                    && q.dfeta_Type.Value == dfeta_qualification_dfeta_Type.MandatoryQualification
                    && q.StateCode == dfeta_qualificationState.Active
                    && q.dfeta_MQ_Date.HasValue)
                .Select(mq => new
                {
                    Awarded = mq.dfeta_MQ_Date.Value.ToDateOnly(),
                    Specialism = mq.Extract<dfeta_specialism>(dfeta_specialism.EntityLogicalName, dfeta_specialism.PrimaryIdAttribute)
                })
                .Where(mq => mq.Specialism != null)
                .Select(mq => new GetTeacherResponseMandatoryQualificationsQualification()
                {
                    Awarded = mq.Awarded,
                    Specialism = mq.Specialism.dfeta_name
                })
                .ToArray() ?? Array.Empty<GetTeacherResponseMandatoryQualificationsQualification>();
    }
}
