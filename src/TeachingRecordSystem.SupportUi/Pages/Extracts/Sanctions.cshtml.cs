using System.Text;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Extracts;

[Authorize(Roles = UserRoles.Administrator)]
public partial class Sanctions(TrsDbContext context, TimeProvider timeProvider) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetNewSanctionsAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var rows = await context.Database
            .SqlQuery<SanctionExtractRow>(
                $"""
                 SELECT
                     p.trn AS "Trn",
                     p.date_of_birth AS "date_of_birth",
                     CASE
                         WHEN p.middle_name IS NULL THEN CONCAT(p.first_name, ' ', p.last_name)
                         ELSE CONCAT(p.first_name, ' ', p.middle_name, ' ', p.last_name)
                     END AS "full_name",
                     t.name AS "sanction_name",
                     a.start_date AS "alert_start_date",
                     NULL::date AS "alert_end_date"
                 FROM persons p
                 INNER JOIN alerts a
                     ON p.person_id = a.person_id
                 LEFT JOIN alert_types t
                     ON a.alert_type_id = t.alert_type_id
                 WHERE
                     p.status = 0
                     AND t.dqt_sanction_code IN ('T1', 'T2', 'T3', 'T4', 'T5')
                     AND a.deleted_on IS NULL
                     AND a.end_date IS NULL
                     AND (
                         a.start_date > ({now.Date}::date - INTERVAL '3 weeks')
                         OR a.created_on > ({now} - INTERVAL '1 week')
                     )
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(rows, $"new-sanctions-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetSpentSanctionsAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var rows = await context.Database
            .SqlQuery<SanctionExtractRow>(
                $"""
                   SELECT
                       p.trn AS "Trn",
                       p.date_of_birth AS "date_of_birth",
                       CASE
                           WHEN p.middle_name IS NULL THEN CONCAT(p.first_name, ' ', p.last_name)
                           ELSE CONCAT(p.first_name, ' ', p.middle_name, ' ', p.last_name)
                       END AS "full_name",
                       t.name AS "sanction_name",
                       a.start_date AS "alert_start_date",
                       a.end_date AS "alert_end_date"
                   FROM persons p
                   INNER JOIN alerts a
                       ON p.person_id = a.person_id
                   LEFT JOIN alert_types t
                       ON a.alert_type_id = t.alert_type_id
                   WHERE
                       p.status = 0
                       AND t.dqt_sanction_code IN ('T1', 'T2', 'T3', 'T4', 'T5')
                       AND a.deleted_on IS NULL
                       AND a.end_date IS NOT NULL
                       AND a.end_date BETWEEN
                            {now.Date} - INTERVAL '3 weeks'
                            AND {now.Date}
                   ORDER BY p.trn
                   """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(rows, $"spent-sanctions-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetAuditAlertsAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var oneMonthAgo = now.AddMonths(-1);

        var rows = await context.Database
            .SqlQuery<AuditingAlertExtractRow>(
                $"""
                 SELECT DISTINCT
                     a.updated_on AS "date_of_change",
                     e.event_name AS "event_type",
                     p.trn AS "teacher_trn",
                     atype.name AS "alert",
                     a.start_date AS "alert_start",
                     a.end_date AS "alert_end",
                     a.created_on AS "alert_created",
                     a.updated_on AS "alert_updated"
                 FROM persons p
                     INNER JOIN alerts a
                         ON a.person_id = p.person_id
                     INNER JOIN alert_types atype
                         ON a.alert_type_id = atype.alert_type_id
                     LEFT OUTER JOIN events e
                         ON e.person_id = p.person_id
                 WHERE
                     a.updated_on >= {oneMonthAgo}
                     AND a.updated_on < {now}
                     AND atype.dqt_sanction_code NOT IN ('E3')
                     AND atype.name NOT IN (
                         'Prohibition by the Secretary of State - misconduct',
                         'Secretary of State decision - no prohibition',
                         'Interim prohibition by the Secretary of State')
                 ORDER BY
                     a.updated_on ASC
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(rows, $"auditing-alerts-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDupSanctionsAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Database
            .SqlQuery<DuplicateSanctionExtractRow>(
                $"""
                 WITH temp AS
                 (
                     SELECT
                         a.person_id,
                         a.alert_type_id
                     FROM alerts a
                     WHERE
                         a.deleted_on IS NULL
                         AND a.end_date IS NULL
                     GROUP BY
                         a.person_id,
                         a.alert_type_id
                     HAVING COUNT(*) > 1
                 )

                 SELECT
                     p.trn AS "trn",
                     CONCAT(p.first_name, ' ', p.last_name) AS "full_name",
                     p.date_of_birth AS "date_of_birth",
                     atype.name AS "alert_name",
                     a.start_date AS "alert_start_date",
                     a.end_date AS "alert_end_date",
                     a.created_on AS "alert_created_on"
                 FROM persons p
                     INNER JOIN alerts a
                         ON p.person_id = a.person_id
                     INNER JOIN temp t
                         ON t.person_id = p.person_id
                         AND t.alert_type_id = a.alert_type_id
                     LEFT OUTER JOIN alert_types atype
                         ON a.alert_type_id = atype.alert_type_id
                 WHERE
                     p.status = 0
                     AND a.deleted_on IS NULL
                     AND a.end_date IS NULL
                 ORDER BY
                     p.trn,
                     a.start_date,
                     a.end_date
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(rows, $"dup-sanctions-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS07DuplicationRecordsWithAlertsAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Database
            .SqlQuery<DqS07DuplicationRecordsWithAlertsExtractRow>(
                $"""
                 WITH temp AS
                 (
                     SELECT
                         p.first_name AS "first_name",
                         p.last_name AS "last_name",
                         p.date_of_birth AS "date_of_birth",
                         p.trn AS "trn",
                         p.national_insurance_number AS "national_insurance_number",
                         p.person_id AS "person_id",
                         atype.name AS "alert",
                         a.start_date AS "alert_start",
                         a.end_date AS "alert_end",
                         a.created_on AS "alert_added_to_dqt"
                     FROM persons p
                         INNER JOIN alerts a
                             ON a.person_id = p.person_id
                         LEFT OUTER JOIN alert_types atype
                             ON a.alert_type_id = atype.alert_type_id
                     WHERE
                         p.status = 0
                         AND p.trn IS NOT NULL
                         AND p.trn NOT IN ('6951863', '7077421', '8946224', '2158492')
                         AND p.last_name IS NOT NULL
                         AND p.first_name IS NOT NULL
                         AND p.date_of_birth IS NOT NULL
                         AND a.end_date IS NULL
                 )

                 SELECT
                     t.trn AS "trn",
                     t.first_name AS "first_name",
                     t.last_name AS "last_name",
                     t.date_of_birth AS "date_of_birth",
                     t.national_insurance_number AS "national_insurance_number",
                     t.alert AS "alert",
                     t.alert_start AS "alert_start",
                     t.alert_end AS "alert_end",
                     t.alert_added_to_dqt AS "alert_added_to_dqt",
                     p.trn AS "trns",
                     p.first_name AS "firstnames",
                     p.last_name AS "surnames",
                     p.date_of_birth AS "dobs",
                     p.national_insurance_number AS "ninos"
                 FROM temp t
                     INNER JOIN persons p
                         ON t.last_name = p.last_name
                         AND t.first_name = p.first_name
                         AND t.date_of_birth = p.date_of_birth
                         AND t.person_id <> p.person_id
                 WHERE
                     p.status = 0
                     AND p.trn IS NOT NULL
                     AND p.last_name IS NOT NULL
                     AND p.first_name IS NOT NULL
                     AND p.date_of_birth IS NOT NULL
                     AND p.trn NOT IN ('1538287', '0048700')
                 ORDER BY
                     p.last_name,
                     p.first_name,
                     p.date_of_birth
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(
            rows,
            $"dq-s07-duplication-records-with-alerts-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS08IpoWithAlertDetailsAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Database
            .SqlQuery<DqS08IpoWithAlertDetailsExtractRow>(
                $"""
                 SELECT
                     p.trn AS "trn",
                     p.first_name AS "first_name",
                     p.middle_name AS "middle_name",
                     p.last_name AS "last_name",
                     p.date_of_birth AS "date_of_birth",
                     CONCAT(p.first_name, ' ', p.last_name) AS "full_name",
                     atype.name AS "alert",
                     a.start_date AS "alert_start",
                     a.end_date AS "alert_end",
                     a.created_on AS "alert_added_to_dqt",
                     a.details AS "details"
                 FROM persons p
                     INNER JOIN alerts a
                         ON p.person_id = a.person_id
                     LEFT OUTER JOIN alert_types atype
                         ON a.alert_type_id = atype.alert_type_id
                 WHERE
                     p.status = 0
                     AND atype.dqt_sanction_code IN ('T2')
                     AND a.details IS NOT NULL
                     AND a.deleted_on IS NULL
                     AND a.end_date IS NULL
                 ORDER BY
                     p.trn
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(
            rows,
            $"dq-s08-ipo-with-alert-details-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS10SoSNoProhibitionsActivePast2YearDateAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var rows = await context.Database
            .SqlQuery<DqS10SoSNoProhibitionsActivePast2YearDateExtractRow>(
                $"""
                 SELECT
                     p.trn AS "trn",
                     p.first_name AS "first_name",
                     p.middle_name AS "middle_name",
                     p.last_name AS "last_name",
                     CONCAT(p.first_name, ' ', p.last_name) AS "full_name",
                     t.name AS "alert",
                     a.start_date AS "alert_start",
                     a.end_date AS "alert_end",
                     a.created_on AS "alert_added_to_dqt",
                     a.details AS "details"
                 FROM persons p
                     INNER JOIN alerts a
                         ON p.person_id = a.person_id
                     LEFT OUTER JOIN alert_types t
                         ON a.alert_type_id = t.alert_type_id
                 WHERE
                     p.status = 0
                     AND a.deleted_on IS NULL
                     AND a.end_date IS NULL
                     AND t.dqt_sanction_code IN ('T6')
                     AND (
                         a.start_date < ({now.Date} - INTERVAL '2 years')
                         OR a.created_on < ({now} - INTERVAL '2 years')
                     )
                 ORDER BY
                     p.trn
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(
            rows,
            $"dq-s10-sos-no-prohibitions-active-past-2-year-date-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS11FailedInductionNoAlertsAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Database
            .SqlQuery<DqS11FailedInductionNoAlertsExtractRow>(
                $"""
                 SELECT DISTINCT
                     p.trn AS "trn",
                     p.first_name AS "first_name",
                     p.last_name AS "last_name",
                     CASE
                         WHEN p.induction_status = 0 THEN 'None'
                         WHEN p.induction_status = 1 THEN 'Required to complete'
                         WHEN p.induction_status = 2 THEN 'Exempt'
                         WHEN p.induction_status = 3 THEN 'In progress'
                         WHEN p.induction_status = 4 THEN 'Passed'
                         WHEN p.induction_status = 5 THEN 'Failed'
                         WHEN p.induction_status = 6 THEN 'Failed in Wales'
                         ELSE ''
                     END AS "induction_status",
                     p.induction_start_date AS "induction_start_date",
                     p.induction_completed_date AS "induction_completed_date"
                 FROM persons p
                 WHERE
                     p.deleted_on IS NULL
                     AND p.induction_status = 5
                     AND p.person_id NOT IN (
                         SELECT DISTINCT a.person_id
                         FROM alerts a
                             INNER JOIN alert_types atype
                                 ON a.alert_type_id = atype.alert_type_id
                         WHERE
                             a.deleted_on IS NULL
                             AND atype.dqt_sanction_code IN ('C2')
                             AND a.end_date IS NULL
                     )
                     AND p.trn NOT IN ('2051301', '2156374')
                 ORDER BY
                     p.induction_completed_date DESC
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(
            rows,
            $"dq-s11-failed-induction-no-alerts-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS12DeletedAlertMonthlyAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var oneMonthAgo = now.AddMonths(-1);

        var rows = await context.Database
            .SqlQuery<DqS12DeletedAlertMonthlyExtractRow>(
                $"""
                 SELECT DISTINCT
                     p.trn AS "trn",
                     p.first_name AS "first_name",
                     p.middle_name AS "middle_name",
                     p.last_name AS "last_name",
                     CONCAT(p.first_name, ' ', p.last_name) AS "full_name",
                     t.name AS "alert",
                     a.start_date AS "alert_start",
                     a.end_date AS "alert_end",
                     a.deleted_on AS "alert_deleted",
                     a.updated_on AS "alert_updated"
                 FROM persons p
                     INNER JOIN alerts a
                         ON p.person_id = a.person_id
                     LEFT OUTER JOIN alert_types t
                         ON a.alert_type_id = t.alert_type_id
                 WHERE
                     a.deleted_on >= {oneMonthAgo}
                     AND a.deleted_on < {now}
                     AND p.status = 0
                     AND p.trn NOT IN ('1434727')
                     AND t.dqt_sanction_code IN ('T2', 'T6', 'T1', 'G1')
                 ORDER BY
                     a.deleted_on DESC
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(
            rows,
            $"dq-s12-deleted-alert-monthly-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetMonthlyTmuAlertReconciliationAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var oneMonthAgo = now.AddMonths(-1);

        var rows = await context.Database
            .SqlQuery<MonthlyTmuAlertReconciliationExtractRow>(
                $"""
                 SELECT
                     p.trn AS "trn",
                     CONCAT(p.first_name, ' ', p.last_name) AS "full_name",
                     p.date_of_birth AS "dob",
                     t.name AS "alert_type",
                     a.start_date AS "alert_start",
                     a.end_date AS "alert_end",
                     a.created_on AS "alert_added_to_trs"
                 FROM persons p
                     INNER JOIN alerts a
                         ON p.person_id = a.person_id
                     INNER JOIN alert_types t
                         ON a.alert_type_id = t.alert_type_id
                 WHERE
                     p.status = 0
                     AND a.deleted_on IS NULL
                     AND a.start_date >= {oneMonthAgo}
                     AND t.dqt_sanction_code IN ('T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T8')
                 ORDER BY
                     a.start_date DESC
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(
            rows,
            $"monthly-tmu-alert-reconciliation-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetMonthlyTmuAlertReconciliationKpiProcessAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var oneMonthAgo = now.AddMonths(-1);

        var rows = await context.Database
            .SqlQuery<MonthlyTmuAlertReconciliationKpiProcessExtractRow>(
                $"""
                 SELECT
                     p.trn AS "trn",
                     CONCAT(p.first_name, ' ', p.last_name) AS "full_name",
                     p.date_of_birth AS "dob",
                     t.name AS "alert_type",
                     a.start_date AS "alert_start",
                     a.end_date AS "alert_end",
                     a.created_on AS "alert_added_to_trs",
                     a.updated_on AS "alert_modified_on_trs"
                 FROM persons p
                     INNER JOIN alerts a
                         ON p.person_id = a.person_id
                     INNER JOIN alert_types t
                         ON a.alert_type_id = t.alert_type_id
                 WHERE
                     p.status = 0
                     AND p.trn IS NOT NULL
                     AND a.deleted_on IS NULL
                     AND a.start_date >= {oneMonthAgo}
                     AND t.dqt_sanction_code IN ('T1', 'T2', 'T6')
                 ORDER BY
                     a.start_date DESC
                 """
            )
            .ToListAsync(cancellationToken);

        return CreateCsv(
            rows,
            $"monthly-tmu-alert-reconciliation-kpi-process-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    private FileStreamResult CreateCsv<T>(
        IEnumerable<T> rows,
        string fileName)
        where T : class
    {
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        using (var csv = CsvWriter.Create(writer))
        {
            csv.WriteRecords(rows);
        }

        stream.Position = 0;

        return File(
            stream,
            "text/csv",
            fileName);
    }

    [CsvRecord]
    public partial record SanctionExtractRow(
        [CsvColumn("TRN")] string? Trn,
        [CsvColumn("Date of birth")][CsvFormat("MM/dd/yyyy")] DateOnly? DateOfBirth,
        [CsvColumn("Full name")] string? FullName,
        [CsvColumn("Sanction name")] string? SanctionName,
        [CsvColumn("Alert start date")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertStartDate,
        [CsvColumn("Alert end date")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertEndDate);

    [CsvRecord]
    public partial record AuditingAlertExtractRow(
        [CsvColumn("Date of change")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? DateOfChange,
        [CsvColumn("EventType")] string? EventType,
        [CsvColumn("Teacher TRN")] string? TeacherTrn,
        [CsvColumn("alert")] string? Alert,
        [CsvColumn("alert_start")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertStart,
        [CsvColumn("alert_end")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertEnd,
        [CsvColumn("alert_created")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertCreated,
        [CsvColumn("alert_updated")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertUpdated);

    [CsvRecord]
    public partial record DuplicateSanctionExtractRow(
        [CsvColumn("TRN")] string? Trn,
        [CsvColumn("Full name")] string? FullName,
        [CsvColumn("Date of birth")][CsvFormat("MM/dd/yyyy")] DateOnly? DateOfBirth,
        [CsvColumn("Alert name")] string? AlertName,
        [CsvColumn("Alert start date")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertStartDate,
        [CsvColumn("Alert end date")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertEndDate,
        [CsvColumn("Alert created on")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertCreatedOn);

    [CsvRecord]
    public partial record DqS07DuplicationRecordsWithAlertsExtractRow(
        [CsvColumn("TRN")] string? Trn,
        [CsvColumn("First name")] string? FirstName,
        [CsvColumn("Last name")] string? LastName,
        [CsvColumn("Date of birth")][CsvFormat("MM/dd/yyyy")] DateOnly? DateOfBirth,
        [CsvColumn("National insurance number")] string? NationalInsuranceNumber,
        [CsvColumn("alert")] string? Alert,
        [CsvColumn("alert_start")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertStart,
        [CsvColumn("alert_end")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertEnd,
        [CsvColumn("alert_addedtodqt")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertAddedToDqt,
        [CsvColumn("TRNS")] string? Trns,
        [CsvColumn("firstnames")] string? Firstnames,
        [CsvColumn("surnames")] string? Surnames,
        [CsvColumn("DOBS")][CsvFormat("MM/dd/yyyy")] DateOnly? Dobs,
        [CsvColumn("NINOS")] string? Ninos);

    [CsvRecord]
    public partial record DqS08IpoWithAlertDetailsExtractRow(
        [CsvColumn("TRN")] string? Trn,
        [CsvColumn("First name")] string? FirstName,
        [CsvColumn("Middle name")] string? MiddleName,
        [CsvColumn("Last name")] string? LastName,
        [CsvColumn("Date of birth")][CsvFormat("MM/dd/yyyy")] DateOnly? DateOfBirth,
        [CsvColumn("Full name")] string? FullName,
        [CsvColumn("alert")] string? Alert,
        [CsvColumn("alert_start")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertStart,
        [CsvColumn("alert_end")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertEnd,
        [CsvColumn("alert_addedtodqt")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertAddedToDqt,
        [CsvColumn("details")] string? Details);

    [CsvRecord]
    public partial record DqS10SoSNoProhibitionsActivePast2YearDateExtractRow(
        [CsvColumn("TRN")] string? Trn,
        [CsvColumn("First name")] string? FirstName,
        [CsvColumn("Middle name")] string? MiddleName,
        [CsvColumn("Last name")] string? LastName,
        [CsvColumn("Full name")] string? FullName,
        [CsvColumn("alert")] string? Alert,
        [CsvColumn("alert_start")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertStart,
        [CsvColumn("alert_end")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertEnd,
        [CsvColumn("alert_addedtodqt")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertAddedToDqt,
        [CsvColumn("details")] string? Details);

    [CsvRecord]
    public partial record DqS11FailedInductionNoAlertsExtractRow(
        [CsvColumn("TRN")] string? Trn,
        [CsvColumn("First name")] string? FirstName,
        [CsvColumn("Last name")] string? LastName,
        [CsvColumn("induction_status")] string? InductionStatus,
        [CsvColumn("induction_start_date")][CsvFormat("MM/dd/yyyy")] DateOnly? InductionStartDate,
        [CsvColumn("induction_completed_date")][CsvFormat("MM/dd/yyyy")] DateOnly? InductionCompletedDate);

    [CsvRecord]
    public partial record DqS12DeletedAlertMonthlyExtractRow(
        [CsvColumn("TRN")] string? Trn,
        [CsvColumn("First name")] string? FirstName,
        [CsvColumn("Middle name")] string? MiddleName,
        [CsvColumn("Last name")] string? LastName,
        [CsvColumn("Full name")] string? FullName,
        [CsvColumn("alert")] string? Alert,
        [CsvColumn("alert_start")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertStart,
        [CsvColumn("alert_end")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertEnd,
        [CsvColumn("alert_deleted")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertDeleted,
        [CsvColumn("alert_updated")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertUpdated);

    [CsvRecord]
    public partial record MonthlyTmuAlertReconciliationExtractRow(
        [CsvColumn("TRN")] string? Trn,
        [CsvColumn("Full name")] string? FullName,
        [CsvColumn("DOB")][CsvFormat("MM/dd/yyyy")] DateOnly? Dob,
        [CsvColumn("Alert type")] string? AlertType,
        [CsvColumn("Alert start")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertStart,
        [CsvColumn("Alert end")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertEnd,
        [CsvColumn("Alert added to TRS")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertAddedToTrs);

    [CsvRecord]
    public partial record MonthlyTmuAlertReconciliationKpiProcessExtractRow(
        [CsvColumn("TRN")] string? Trn,
        [CsvColumn("Full name")] string? FullName,
        [CsvColumn("DOB")][CsvFormat("MM/dd/yyyy")] DateOnly? Dob,
        [CsvColumn("Alert type")] string? AlertType,
        [CsvColumn("Alert start")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertStart,
        [CsvColumn("Alert end")][CsvFormat("MM/dd/yyyy")] DateOnly? AlertEnd,
        [CsvColumn("Alert added to TRS")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertAddedToTrs,
        [CsvColumn("Alert modified on TRS")][CsvFormat("MM/dd/yyyy HH:mm:ss")] DateTime? AlertModifiedOnTrs);
}
