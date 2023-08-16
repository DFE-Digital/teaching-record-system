#nullable disable
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt;

public partial class DataverseAdapter
{
    public async Task<SetNpqQualificationResult> SetNpqQualification(SetNpqQualificationCommand command)
    {
        var (r, _) = await SetNpqQualificationImpl(command);
        return r;
    }

    internal async Task<(SetNpqQualificationResult, ExecuteTransactionRequest TransactionRequest)> SetNpqQualificationImpl(SetNpqQualificationCommand command)
    {
        //all qualifications
        var teacher = await GetTeacher(
            command.TeacherId,
            columnNames: new[]
            {
                Contact.Fields.dfeta_ActiveSanctions,
                Contact.Fields.dfeta_TRN
            });

        var qualifications = await GetQualificationsForTeacher(
            command.TeacherId,
            new[]
            {
                dfeta_qualification.Fields.dfeta_Type,
                dfeta_qualification.Fields.dfeta_createdbyapi
            });

        //existing qualifications for request qualification type
        var existingQualifications = qualifications.Where(x => x.dfeta_Type == command.QualificationType && x.dfeta_createdbyapi == true).ToList();

        // Send a single Transaction request with all the data changes in.
        // This is important for atomicity; we really do not want torn writes here.
        var txnRequest = new ExecuteTransactionRequest()
        {
            ReturnResponses = true,
            Requests = new()
        };

        if (existingQualifications.Count == 0)
        {
            if (command.CompletionDate.HasValue)
            {
                //create new qualification
                var createQual = CreateQualificationEntity(null, command.QualificationType, command.TeacherId);
                var completionDatefieldName = GetNpqDateFieldNameForQualificationType(command.QualificationType);
                var awardedfieldName = GetNpqAwardedFieldNameForQualificationType(command.QualificationType);

                createQual[completionDatefieldName] = command.CompletionDate;
                createQual[awardedfieldName] = command.CompletionDate.HasValue ? true : false;
                createQual.dfeta_createdbyapi = true;
                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = createQual
                });
            }
            else
            {
                return (SetNpqQualificationResult.Failed(SetNpqQualificationFailedReasons.NpqQualificationNotCreatedByApi), null);
            }
        }
        else if (existingQualifications.Count == 1)
        {
            //update existing qualification
            var updateQual = CreateQualificationEntity(existingQualifications[0].Id, null, null);
            var completionDatefieldName = GetNpqDateFieldNameForQualificationType(existingQualifications[0].dfeta_Type.Value);
            var awardedfieldName = GetNpqAwardedFieldNameForQualificationType(existingQualifications[0].dfeta_Type.Value);

            updateQual[completionDatefieldName] = command.CompletionDate;
            updateQual[awardedfieldName] = command.CompletionDate.HasValue ? true : false;
            txnRequest.Requests.Add(new UpdateRequest()
            {
                Target = updateQual
            });
        }
        else
        {
            return (SetNpqQualificationResult.Failed(SetNpqQualificationFailedReasons.MultipleNpqQualificationsWithQualificationType), null);
        }

        // teacher has a sanction, create review task.
        if (teacher.dfeta_ActiveSanctions == true)
        {
            var reviewTask = CreateReviewTaskForActiveSanctions(command.TeacherId, teacher.dfeta_TRN);
            txnRequest.Requests.Add(new CreateRequest()
            {
                Target = reviewTask
            });
        }

        await _service.ExecuteAsync(txnRequest);
        return (SetNpqQualificationResult.Success(), txnRequest);
    }

    public Models.Task CreateReviewTaskForActiveSanctions(Guid teacherId, string trn)
    {
        var description = GetDescription();

        return new Models.Task()
        {
            RegardingObjectId = teacherId.ToEntityReference(Contact.EntityLogicalName),
            Category = "NPQImport",
            Subject = "NPQ Import: Teacher has active alert",
            Description = description,
            ScheduledEnd = _clock.UtcNow
        };

        string GetDescription()
        {
            string desc = $"{trn} has active sanctions";
            return desc;
        }
    }

    private string GetNpqDateFieldNameForQualificationType(dfeta_qualification_dfeta_Type type) => type switch
    {
        dfeta_qualification_dfeta_Type.NPQLBC => dfeta_qualification.Fields.dfeta_npqlbc_date,
        dfeta_qualification_dfeta_Type.NPQLTD => dfeta_qualification.Fields.dfeta_npqltd_date,
        dfeta_qualification_dfeta_Type.NPQLT => dfeta_qualification.Fields.dfeta_npqlt_date,
        dfeta_qualification_dfeta_Type.NPQEL => dfeta_qualification.Fields.dfeta_NPQEL_Date,
        dfeta_qualification_dfeta_Type.NPQML => dfeta_qualification.Fields.dfeta_npqml_date,
        dfeta_qualification_dfeta_Type.NPQH => dfeta_qualification.Fields.dfeta_NPQH_Date,
        dfeta_qualification_dfeta_Type.NPQSL => dfeta_qualification.Fields.dfeta_npqsl_date,
        dfeta_qualification_dfeta_Type.NPQEYL => dfeta_qualification.Fields.dfeta_npqeyl_date,
        dfeta_qualification_dfeta_Type.NPQLL => dfeta_qualification.Fields.dfeta_npqll_date,
        _ => throw new FormatException($"Unknown {nameof(type)}: '{type}'.")
    };

    private string GetNpqAwardedFieldNameForQualificationType(dfeta_qualification_dfeta_Type type) => type switch
    {
        dfeta_qualification_dfeta_Type.NPQLBC => dfeta_qualification.Fields.dfeta_npqlbc_awarded,
        dfeta_qualification_dfeta_Type.NPQLTD => dfeta_qualification.Fields.dfeta_npqltd_awarded,
        dfeta_qualification_dfeta_Type.NPQLT => dfeta_qualification.Fields.dfeta_npqlt_awarded,
        dfeta_qualification_dfeta_Type.NPQEYL => dfeta_qualification.Fields.dfeta_npqeyl_awarded,
        dfeta_qualification_dfeta_Type.NPQLL => dfeta_qualification.Fields.dfeta_npqll_awarded,
        dfeta_qualification_dfeta_Type.NPQEL => dfeta_qualification.Fields.dfeta_NPQEL_Awarded,
        dfeta_qualification_dfeta_Type.NPQML => dfeta_qualification.Fields.dfeta_npqml_awarded,
        dfeta_qualification_dfeta_Type.NPQH => dfeta_qualification.Fields.dfeta_NPQH_Awarded,
        dfeta_qualification_dfeta_Type.NPQSL => dfeta_qualification.Fields.dfeta_npqsl_awarded,
        _ => throw new FormatException($"Unknown {nameof(type)}: '{type}'.")
    };

    public dfeta_qualification CreateQualificationEntity(Guid? id, dfeta_qualification_dfeta_Type? qualificationType, Guid? teacherId)
    {
        var entity = new dfeta_qualification()
        {
            Id = id ?? Guid.NewGuid(),
            dfeta_Type = qualificationType,
            dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, teacherId ?? Guid.Empty),
        };

        if (!teacherId.HasValue)
        {
            entity.Attributes.Remove(dfeta_qualification.Fields.dfeta_PersonId);
        }

        if (!qualificationType.HasValue)
        {
            entity.Attributes.Remove(dfeta_qualification.Fields.dfeta_Type);
        }

        return entity;
    }
}
