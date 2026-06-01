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

        return CreateCsv(rows, $"new-sanctions-{timeProvider.UtcNow:yyyyMMdd}.csv");
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

        return CreateCsv(rows, $"spent-sanctions-{timeProvider.UtcNow:yyyyMMdd}.csv");
    }

    private FileStreamResult CreateCsv(
        IEnumerable<SanctionExtractRow> rows,
        string fileName)
    {
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<SanctionExtractRowMap>();
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
}
