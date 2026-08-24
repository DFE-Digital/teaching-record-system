using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Extracts;

[Authorize(Roles = UserRoles.Administrator)]
public class Sanctions(TrsDbContext context, TimeProvider timeProvider) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetNewSanctionsAsync()
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
            .ToListAsync();

        return CreateCsv(rows, new SanctionExtractRowMap(), $"new-sanctions-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetSpentSanctionsAsync()
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
            .ToListAsync();

        return CreateCsv(rows, new SanctionExtractRowMap(), $"spent-sanctions-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetAuditAlertsAsync()
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
            .ToListAsync();

        return CreateCsv(rows, new AuditingAlertExtractRowMap(), $"auditing-alerts-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDupSanctionsAsync()
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
            .ToListAsync();

        return CreateCsv(rows, new DuplicateSanctionExtractRowMap(), $"dup-sanctions-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS07DuplicationRecordsWithAlertsAsync()
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
            .ToListAsync();

        return CreateCsv(
            rows,
            new DqS07DuplicationRecordsWithAlertsExtractRowMap(),
            $"dq-s07-duplication-records-with-alerts-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS08IpoWithAlertDetailsAsync()
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
            .ToListAsync();

        return CreateCsv(
            rows,
            new DqS08IpoWithAlertDetailsExtractRowMap(),
            $"dq-s08-ipo-with-alert-details-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS10SoSNoProhibitionsActivePast2YearDateAsync()
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
            .ToListAsync();

        return CreateCsv(
            rows,
            new DqS10SoSNoProhibitionsActivePast2YearDateExtractRowMap(),
            $"dq-s10-sos-no-prohibitions-active-past-2-year-date-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS11FailedInductionNoAlertsAsync()
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
            .ToListAsync();

        return CreateCsv(
            rows,
            new DqS11FailedInductionNoAlertsExtractRowMap(),
            $"dq-s11-failed-induction-no-alerts-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetDqS12DeletedAlertMonthlyAsync()
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
            .ToListAsync();

        return CreateCsv(
            rows,
            new DqS12DeletedAlertMonthlyExtractRowMap(),
            $"dq-s12-deleted-alert-monthly-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetMonthlyTmuAlertReconciliationAsync()
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
            .ToListAsync();

        return CreateCsv(
            rows,
            new MonthlyTmuAlertReconciliationExtractRowMap(),
            $"monthly-tmu-alert-reconciliation-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> OnGetMonthlyTmuAlertReconciliationKpiProcessAsync()
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
            .ToListAsync();

        return CreateCsv(
            rows,
            new MonthlyTmuAlertReconciliationKpiProcessExtractRowMap(),
            $"monthly-tmu-alert-reconciliation-kpi-process-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    private FileStreamResult CreateCsv<T>(
        IEnumerable<T> rows,
        ClassMap<T> classMap,
        string fileName)
        where T : class
    {
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap(classMap);
            csv.WriteRecords(rows);
        }

        stream.Position = 0;

        return File(
            stream,
            "text/csv",
            fileName);
    }

    public record SanctionExtractRow(
        string? Trn,
        DateOnly? DateOfBirth,
        string? FullName,
        string? SanctionName,
        DateOnly? AlertStartDate,
        DateOnly? AlertEndDate);

    public record AuditingAlertExtractRow(
        DateTime? DateOfChange,
        string? EventType,
        string? TeacherTrn,
        string? Alert,
        DateOnly? AlertStart,
        DateOnly? AlertEnd,
        DateTime? AlertCreated,
        DateTime? AlertUpdated);

    public record DuplicateSanctionExtractRow(
        string? Trn,
        string? FullName,
        DateOnly? DateOfBirth,
        string? AlertName,
        DateOnly? AlertStartDate,
        DateOnly? AlertEndDate,
        DateTime? AlertCreatedOn);

    public record DqS07DuplicationRecordsWithAlertsExtractRow(
        string? Trn,
        string? FirstName,
        string? LastName,
        DateOnly? DateOfBirth,
        string? NationalInsuranceNumber,
        string? Alert,
        DateOnly? AlertStart,
        DateOnly? AlertEnd,
        DateTime? AlertAddedToDqt,
        string? Trns,
        string? Firstnames,
        string? Surnames,
        DateOnly? Dobs,
        string? Ninos);

    public record DqS08IpoWithAlertDetailsExtractRow(
        string? Trn,
        string? FirstName,
        string? MiddleName,
        string? LastName,
        DateOnly? DateOfBirth,
        string? FullName,
        string? Alert,
        DateOnly? AlertStart,
        DateOnly? AlertEnd,
        DateTime? AlertAddedToDqt,
        string? Details);

    public record DqS10SoSNoProhibitionsActivePast2YearDateExtractRow(
        string? Trn,
        string? FirstName,
        string? MiddleName,
        string? LastName,
        string? FullName,
        string? Alert,
        DateOnly? AlertStart,
        DateOnly? AlertEnd,
        DateTime? AlertAddedToDqt,
        string? Details);

    public record DqS11FailedInductionNoAlertsExtractRow(
        string? Trn,
        string? FirstName,
        string? LastName,
        string? InductionStatus,
        DateOnly? InductionStartDate,
        DateOnly? InductionCompletedDate);

    public record DqS12DeletedAlertMonthlyExtractRow(
        string? Trn,
        string? FirstName,
        string? MiddleName,
        string? LastName,
        string? FullName,
        string? Alert,
        DateOnly? AlertStart,
        DateOnly? AlertEnd,
        DateTime? AlertDeleted,
        DateTime? AlertUpdated);

    public record MonthlyTmuAlertReconciliationExtractRow(
        string? Trn,
        string? FullName,
        DateOnly? Dob,
        string? AlertType,
        DateOnly? AlertStart,
        DateOnly? AlertEnd,
        DateTime? AlertAddedToTrs);

    public record MonthlyTmuAlertReconciliationKpiProcessExtractRow(
        string? Trn,
        string? FullName,
        DateOnly? Dob,
        string? AlertType,
        DateOnly? AlertStart,
        DateOnly? AlertEnd,
        DateTime? AlertAddedToTrs,
        DateTime? AlertModifiedOnTrs);

    public sealed class SanctionExtractRowMap : ClassMap<SanctionExtractRow>
    {
        public SanctionExtractRowMap()
        {
            Map(m => m.Trn).Name("TRN").Optional();
            Map(m => m.DateOfBirth).Name("Date of birth").Optional();
            Map(m => m.FullName).Name("Full name").Optional();
            Map(m => m.SanctionName).Name("Sanction name").Optional();
            Map(m => m.AlertStartDate).Name("Alert start date").Optional();
            Map(m => m.AlertEndDate).Name("Alert end date").Optional();
        }
    }

    public sealed class AuditingAlertExtractRowMap : ClassMap<AuditingAlertExtractRow>
    {
        public AuditingAlertExtractRowMap()
        {
            Map(m => m.DateOfChange).Name("Date of change").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
            Map(m => m.EventType).Name("EventType").Optional();
            Map(m => m.TeacherTrn).Name("Teacher TRN").Optional();
            Map(m => m.Alert).Name("alert").Optional();
            Map(m => m.AlertStart).Name("alert_start").Optional();
            Map(m => m.AlertEnd).Name("alert_end").Optional();
            Map(m => m.AlertCreated).Name("alert_created").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
            Map(m => m.AlertUpdated).Name("alert_updated").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
        }
    }

    public sealed class DuplicateSanctionExtractRowMap : ClassMap<DuplicateSanctionExtractRow>
    {
        public DuplicateSanctionExtractRowMap()
        {
            Map(m => m.Trn).Name("TRN").Optional();
            Map(m => m.FullName).Name("Full name").Optional();
            Map(m => m.DateOfBirth).Name("Date of birth").Optional();
            Map(m => m.AlertName).Name("Alert name").Optional();
            Map(m => m.AlertStartDate).Name("Alert start date").Optional();
            Map(m => m.AlertEndDate).Name("Alert end date").Optional();
            Map(m => m.AlertCreatedOn).Name("Alert created on").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
        }
    }

    public sealed class DqS07DuplicationRecordsWithAlertsExtractRowMap : ClassMap<DqS07DuplicationRecordsWithAlertsExtractRow>
    {
        public DqS07DuplicationRecordsWithAlertsExtractRowMap()
        {
            Map(m => m.Trn).Name("TRN").Optional();
            Map(m => m.FirstName).Name("First name").Optional();
            Map(m => m.LastName).Name("Last name").Optional();
            Map(m => m.DateOfBirth).Name("Date of birth").Optional();
            Map(m => m.NationalInsuranceNumber).Name("National insurance number").Optional();
            Map(m => m.Alert).Name("alert").Optional();
            Map(m => m.AlertStart).Name("alert_start").Optional();
            Map(m => m.AlertEnd).Name("alert_end").Optional();
            Map(m => m.AlertAddedToDqt).Name("alert_addedtodqt").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
            Map(m => m.Trns).Name("TRNS").Optional();
            Map(m => m.Firstnames).Name("firstnames").Optional();
            Map(m => m.Surnames).Name("surnames").Optional();
            Map(m => m.Dobs).Name("DOBS").Optional();
            Map(m => m.Ninos).Name("NINOS").Optional();
        }
    }

    public sealed class DqS08IpoWithAlertDetailsExtractRowMap : ClassMap<DqS08IpoWithAlertDetailsExtractRow>
    {
        public DqS08IpoWithAlertDetailsExtractRowMap()
        {
            Map(m => m.Trn).Name("TRN").Optional();
            Map(m => m.FirstName).Name("First name").Optional();
            Map(m => m.MiddleName).Name("Middle name").Optional();
            Map(m => m.LastName).Name("Last name").Optional();
            Map(m => m.DateOfBirth).Name("Date of birth").Optional();
            Map(m => m.FullName).Name("Full name").Optional();
            Map(m => m.Alert).Name("alert").Optional();
            Map(m => m.AlertStart).Name("alert_start").Optional();
            Map(m => m.AlertEnd).Name("alert_end").Optional();
            Map(m => m.AlertAddedToDqt).Name("alert_addedtodqt").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
            Map(m => m.Details).Name("details").Optional();
        }
    }

    public sealed class DqS10SoSNoProhibitionsActivePast2YearDateExtractRowMap : ClassMap<DqS10SoSNoProhibitionsActivePast2YearDateExtractRow>
    {
        public DqS10SoSNoProhibitionsActivePast2YearDateExtractRowMap()
        {
            Map(m => m.Trn).Name("TRN").Optional();
            Map(m => m.FirstName).Name("First name").Optional();
            Map(m => m.MiddleName).Name("Middle name").Optional();
            Map(m => m.LastName).Name("Last name").Optional();
            Map(m => m.FullName).Name("Full name").Optional();
            Map(m => m.Alert).Name("alert").Optional();
            Map(m => m.AlertStart).Name("alert_start").Optional();
            Map(m => m.AlertEnd).Name("alert_end").Optional();
            Map(m => m.AlertAddedToDqt).Name("alert_addedtodqt").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
            Map(m => m.Details).Name("details").Optional();
        }
    }

    public sealed class DqS11FailedInductionNoAlertsExtractRowMap : ClassMap<DqS11FailedInductionNoAlertsExtractRow>
    {
        public DqS11FailedInductionNoAlertsExtractRowMap()
        {
            Map(m => m.Trn).Name("TRN").Optional();
            Map(m => m.FirstName).Name("First name").Optional();
            Map(m => m.LastName).Name("Last name").Optional();
            Map(m => m.InductionStatus).Name("induction_status").Optional();
            Map(m => m.InductionStartDate).Name("induction_start_date").Optional();
            Map(m => m.InductionCompletedDate).Name("induction_completed_date").Optional();
        }
    }

    public sealed class DqS12DeletedAlertMonthlyExtractRowMap : ClassMap<DqS12DeletedAlertMonthlyExtractRow>
    {
        public DqS12DeletedAlertMonthlyExtractRowMap()
        {
            Map(m => m.Trn).Name("TRN").Optional();
            Map(m => m.FirstName).Name("First name").Optional();
            Map(m => m.MiddleName).Name("Middle name").Optional();
            Map(m => m.LastName).Name("Last name").Optional();
            Map(m => m.FullName).Name("Full name").Optional();
            Map(m => m.Alert).Name("alert").Optional();
            Map(m => m.AlertStart).Name("alert_start").Optional();
            Map(m => m.AlertEnd).Name("alert_end").Optional();
            Map(m => m.AlertDeleted).Name("alert_deleted").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
            Map(m => m.AlertUpdated).Name("alert_updated").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
        }
    }

    public sealed class MonthlyTmuAlertReconciliationExtractRowMap : ClassMap<MonthlyTmuAlertReconciliationExtractRow>
    {
        public MonthlyTmuAlertReconciliationExtractRowMap()
        {
            Map(m => m.Trn).Name("TRN").Optional();
            Map(m => m.FullName).Name("Full name").Optional();
            Map(m => m.Dob).Name("DOB").Optional();
            Map(m => m.AlertType).Name("Alert type").Optional();
            Map(m => m.AlertStart).Name("Alert start").Optional();
            Map(m => m.AlertEnd).Name("Alert end").Optional();
            Map(m => m.AlertAddedToTrs).Name("Alert added to TRS").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
        }
    }

    public sealed class MonthlyTmuAlertReconciliationKpiProcessExtractRowMap : ClassMap<MonthlyTmuAlertReconciliationKpiProcessExtractRow>
    {
        public MonthlyTmuAlertReconciliationKpiProcessExtractRowMap()
        {
            Map(m => m.Trn).Name("TRN").Optional();
            Map(m => m.FullName).Name("Full name").Optional();
            Map(m => m.Dob).Name("DOB").Optional();
            Map(m => m.AlertType).Name("Alert type").Optional();
            Map(m => m.AlertStart).Name("Alert start").Optional();
            Map(m => m.AlertEnd).Name("Alert end").Optional();
            Map(m => m.AlertAddedToTrs).Name("Alert added to TRS").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
            Map(m => m.AlertModifiedOnTrs).Name("Alert modified on TRS").TypeConverterOption.Format("MM/dd/yyyy HH:mm:ss").Optional();
        }
    }
}
