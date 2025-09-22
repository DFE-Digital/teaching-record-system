using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Jobs;

public class ResetIncorrectHasEypsOnPersonsJob(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IFileService fileService,
    IClock clock)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        var allRouteTypes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync();

        var eypsRouteTypes = allRouteTypes.Where(r => r.ProfessionalStatusType == ProfessionalStatusType.EarlyYearsProfessionalStatus).Select(r => r.RouteToProfessionalStatusTypeId);
        var eytsRouteTypes = allRouteTypes.Where(r => r.ProfessionalStatusType == ProfessionalStatusType.EarlyYearsTeacherStatus).Select(r => r.RouteToProfessionalStatusTypeId);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Get all persons where HasEyps has been set to true
        var personsWithHasEyps = await dbContext.Persons
            .Include(p => p.Qualifications)
            .Where(p => p.HasEyps)
            .ToListAsync(cancellationToken);

        var updatedPersons = new List<Guid>();

        // Update all persons where HasEyps has been set to true but there is no EYPS route
        foreach (var person in personsWithHasEyps.Where(p => !p.Qualifications!.OfType<RouteToProfessionalStatus>().Any(r => eypsRouteTypes.Contains(r.RouteToProfessionalStatusTypeId) && r.Status == RouteToProfessionalStatusStatus.Holds)))
        {
            // Need to use the "wrong" EYTS route that was originally used to set HasEyps in the event
            var eytsRoute = person.Qualifications!.OfType<RouteToProfessionalStatus>().FirstOrDefault(r => eytsRouteTypes.Contains(r.RouteToProfessionalStatusTypeId) && r.Status == RouteToProfessionalStatusStatus.Holds);
            var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person);
            var oldInduction = EventModels.Induction.FromModel(person);
            var newInduction = EventModels.Induction.FromModel(person);

            person.HasEyps = false;

            dbContext.AddEvent(new RouteToProfessionalStatusUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                PersonId = person.PersonId,
                RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId,
                RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(eytsRoute!),
                OldRouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(eytsRoute!),
                ChangeReason = "Data fix for incorrectly set Has EYPS flag",
                ChangeReasonDetail = null,
                EvidenceFile = null,
                PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person),
                OldPersonAttributes = oldPersonAttributes,
                Changes = RouteToProfessionalStatusUpdatedEventChanges.PersonHasEyps,
                Induction = newInduction,
                OldInduction = oldInduction
            });

            updatedPersons.Add(person.PersonId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        if (!dryRun)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        if (updatedPersons.Count == 0)
        {
            return;
        }

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(updatedPersons.Select(p => new { PersonId = p }), cancellationToken);
        await writer.FlushAsync();
        stream.Position = 0;

        await fileService.UploadFileAsync($"resethaseyps{clock.UtcNow:yyyyMMddHHmmss}.csv", stream, "text/csv");
    }
}
