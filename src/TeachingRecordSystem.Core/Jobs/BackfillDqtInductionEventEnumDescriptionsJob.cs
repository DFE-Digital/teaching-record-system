using Npgsql;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillDqtInductionEventEnumDescriptionsJob(NpgsqlDataSource trsDbDataSource)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        var updateInductionStatusSql =
            $"""
             WITH induction_status_title AS (
                SELECT
                    *
                FROM
                    (VALUES
                    ('FailedinWales','Failed in Wales'),
                    ('InductionExtended','Induction Extended'),
                    ('InProgress','In Progress'),
                    ('NotYetCompleted','Not Yet Completed'),
                    ('PassedinWales','Passed in Wales'),
                    ('RequiredtoComplete','Required to Complete')) as t(induction_status, title)
            ),
            changes AS (
                SELECT
                    event_id,
                    t.title
                FROM
                        events e
                    JOIN
                        induction_status_title t ON t.induction_status = (payload -> 'Induction' ->> 'InductionStatus')
                WHERE
                    e.event_name in ('DqtInductionCreatedEvent', 'DqtInductionUpdatedEvent')
                LIMIT 1000
            )
            UPDATE
                events e
            SET
                payload = jsonb_set(payload, array['Induction', 'InductionStatus'], to_jsonb(c.title::text), false)
            FROM
                changes c
            WHERE
                e.event_id = c.event_id;
            """;

        await UpdateDatabaseAsync(updateInductionStatusSql, connection, cancellationToken);

        var updateInductionExemptionReasonSql =
            $"""
            WITH induction_exemption_title AS (
                SELECT
                    *
                FROM
                    (VALUES
                    ('ExemptDataLossErrorCriteria','Exempt - Data Loss/Error Criteria'),
                    ('Extendedonappeal','Extended on appeal'),
                    ('HasoriseligibleforfullregistrationinScotland','Has, or is eligible for, full registration in Scotland'),
                    ('OverseasTrainedTeacher','Overseas Trained Teacher'),
                    ('Qualifiedbefore07May1999','Qualified before 07 May 1999'),
                    ('Qualifiedbetween07May1999and01April2003FirstpostwasinWalesandlastedaminimumoftwoterms','Qualified between 07 May 1999 and 01 April 2003. First post was in Wales and lasted a minimum of two terms.'),		
                    ('QualifiedthroughEEAmutualrecognitionroute','Qualified through EEA mutual recognition route'),
                    ('QualifiedthroughFEroutebetween01Sep2001and01Sep2004','Qualified through FE route between 01 Sep 2001 and 01 Sep 2004'),
                    ('QualifiedthroughIndependentroutebetween01Oct2000and01Sep2004','Qualified through Independent route between 01 Oct 2000 and 01 Sep 2004'),
                    ('RegisteredTeacher_havingatleasttwoyearsfulltimeteachingexperience','Registered Teacher (having at least two year’s full time teaching experience)'),
                    ('SuccessfullycompletedinductioninGuernsey','Successfully completed induction in Guernsey'),
                    ('SuccessfullycompletedinductioninIsleOfMan','Successfully completed induction in Isle Of Man'),
                    ('SuccessfullycompletedinductioninJersey','Successfully completed induction in Jersey'),
                    ('SuccessfullycompletedinductioninNorthernIreland','Successfully completed induction in Northern Ireland'),
                    ('SuccessfullycompletedinductioninServiceChildrensEducationschoolsinGermanyorCyprus','Successfully completed induction in Service Children’s Education schools in Germany or Cyprus'),
                    ('SuccessfullycompletedinductioninWales','Successfully completed induction in Wales'),
                    ('SuccessfullycompletedprobationaryperiodinGibraltar','Successfully completed probationary period in Gibraltar'),
                    ('TeacherhasbeenawardedQTLSandisexemptprovidedtheymaintaintheirmembershipwiththeSocietyforEducationandTraining','Teacher has been awarded QTLS and is exempt provided they maintain their membership with the Society for Education and Training')) as t(induction_exemption_reason, title)
            ),
            changes AS (
                SELECT
                    event_id,
                    t.title
                FROM
                        events e
                    JOIN
                        induction_exemption_title t ON t.induction_exemption_reason = (payload -> 'Induction' ->> 'InductionExemptionReason')
                WHERE
                    e.event_name in ('DqtInductionCreatedEvent', 'DqtInductionUpdatedEvent')
                LIMIT 1000
            )
            UPDATE
                events e
            SET
                payload = jsonb_set(payload, array['Induction', 'InductionExemptionReason'], to_jsonb(c.title::text), false)
            FROM
                changes c
            WHERE
                e.event_id = c.event_id;
            """;

        await UpdateDatabaseAsync(updateInductionExemptionReasonSql, connection, cancellationToken);

        var updateOldInductionStatusSql =
            $"""
            WITH induction_status_title AS (
                SELECT
                    *
                FROM
                    (VALUES
                    ('FailedinWales','Failed in Wales'),
                    ('InductionExtended','Induction Extended'),
                    ('InProgress','In Progress'),
                    ('NotYetCompleted','Not Yet Completed'),
                    ('PassedinWales','Passed in Wales'),
                    ('RequiredtoComplete','Required to Complete')) as t(induction_status, title)
            ),
            changes AS (
                SELECT
                    event_id,
                    t.title
                FROM
                        events e
                    JOIN
                        induction_status_title t ON t.induction_status = (payload -> 'OldInduction' ->> 'InductionStatus')
                WHERE
                    e.event_name = 'DqtInductionUpdatedEvent'
                LIMIT 1000
            )
            UPDATE
                events e
            SET
                payload = jsonb_set(payload, array['OldInduction', 'InductionStatus'], to_jsonb(c.title::text), false)
            FROM
                changes c
            WHERE
                e.event_id = c.event_id;
            """;

        await UpdateDatabaseAsync(updateOldInductionStatusSql, connection, cancellationToken);

        var updateOldInductionExemptionReasonSql =
            $"""
            WITH induction_exemption_title AS (
                SELECT
                    *
                FROM
                    (VALUES
                    ('ExemptDataLossErrorCriteria','Exempt - Data Loss/Error Criteria'),
                    ('Extendedonappeal','Extended on appeal'),
                    ('HasoriseligibleforfullregistrationinScotland','Has, or is eligible for, full registration in Scotland'),
                    ('OverseasTrainedTeacher','Overseas Trained Teacher'),
                    ('Qualifiedbefore07May1999','Qualified before 07 May 1999'),
                    ('Qualifiedbetween07May1999and01April2003FirstpostwasinWalesandlastedaminimumoftwoterms','Qualified between 07 May 1999 and 01 April 2003. First post was in Wales and lasted a minimum of two terms.'),		
                    ('QualifiedthroughEEAmutualrecognitionroute','Qualified through EEA mutual recognition route'),
                    ('QualifiedthroughFEroutebetween01Sep2001and01Sep2004','Qualified through FE route between 01 Sep 2001 and 01 Sep 2004'),
                    ('QualifiedthroughIndependentroutebetween01Oct2000and01Sep2004','Qualified through Independent route between 01 Oct 2000 and 01 Sep 2004'),
                    ('RegisteredTeacher_havingatleasttwoyearsfulltimeteachingexperience','Registered Teacher (having at least two year’s full time teaching experience)'),
                    ('SuccessfullycompletedinductioninGuernsey','Successfully completed induction in Guernsey'),
                    ('SuccessfullycompletedinductioninIsleOfMan','Successfully completed induction in Isle Of Man'),
                    ('SuccessfullycompletedinductioninJersey','Successfully completed induction in Jersey'),
                    ('SuccessfullycompletedinductioninNorthernIreland','Successfully completed induction in Northern Ireland'),
                    ('SuccessfullycompletedinductioninServiceChildrensEducationschoolsinGermanyorCyprus','Successfully completed induction in Service Children’s Education schools in Germany or Cyprus'),
                    ('SuccessfullycompletedinductioninWales','Successfully completed induction in Wales'),
                    ('SuccessfullycompletedprobationaryperiodinGibraltar','Successfully completed probationary period in Gibraltar'),
                    ('TeacherhasbeenawardedQTLSandisexemptprovidedtheymaintaintheirmembershipwiththeSocietyforEducationandTraining','Teacher has been awarded QTLS and is exempt provided they maintain their membership with the Society for Education and Training')) as t(induction_exemption_reason, title)
            ),
            changes AS (
                SELECT
                    event_id,
                    t.title
                FROM
                        events e
                    JOIN
                        induction_exemption_title t ON t.induction_exemption_reason = (payload -> 'OldInduction' ->> 'InductionExemptionReason')
                WHERE
                    e.event_name = 'DqtInductionUpdatedEvent'
                LIMIT 1000
            )
            UPDATE
                events e
            SET
                payload = jsonb_set(payload, array['OldInduction', 'InductionExemptionReason'], to_jsonb(c.title::text), false)
            FROM
                changes c
            WHERE
                e.event_id = c.event_id;
            """;

        await UpdateDatabaseAsync(updateOldInductionExemptionReasonSql, connection, cancellationToken);
    }

    private async Task UpdateDatabaseAsync(string updateSql, NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        bool hasRecordsToUpdate = false;

        do
        {
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            using (var updateCommand = transaction.Connection!.CreateCommand())
            {
                updateCommand.CommandText = updateSql;
                updateCommand.CommandTimeout = 300;
                updateCommand.Transaction = transaction;
                var rowsAffected = await updateCommand.ExecuteNonQueryAsync(cancellationToken);
                hasRecordsToUpdate = rowsAffected > 0;
            }
            await transaction.CommitAsync(cancellationToken);
        } while (hasRecordsToUpdate);
    }
}
