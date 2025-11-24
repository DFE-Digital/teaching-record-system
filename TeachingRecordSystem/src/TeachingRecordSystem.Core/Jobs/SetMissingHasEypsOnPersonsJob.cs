using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Jobs;

public class SetMissingHasEypsOnPersonsJob(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IFileService fileService,
    IClock clock)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        var allRouteTypes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync();
        var eypsRouteTypes = allRouteTypes.Where(r => r.ProfessionalStatusType == ProfessionalStatusType.EarlyYearsProfessionalStatus).Select(r => r.RouteToProfessionalStatusTypeId);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Get all EYPS routes
        var eypsRoutes = await dbContext.RouteToProfessionalStatuses
            .Include(r => r.Person)
            .Where(r => eypsRouteTypes.Contains(r.RouteToProfessionalStatusTypeId) && r.Status == RouteToProfessionalStatusStatus.Holds)
            .ToListAsync(cancellationToken);

        var updatedPersons = new List<Guid>();

        // Update all persons where HasEyps has been set to false but there is an EYPS route
        foreach (var route in eypsRoutes.Where(r => r.Person != null && r.Person.HasEyps == false))
        {
            var person = route.Person!;
            var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person);
            var oldInduction = EventModels.Induction.FromModel(person);
            var newInduction = EventModels.Induction.FromModel(person);

            person.HasEyps = true;

            dbContext.AddEventWithoutBroadcast(new RouteToProfessionalStatusUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                PersonId = person.PersonId,
                RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId,
                RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(route),
                OldRouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(route),
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
        await writer.FlushAsync(cancellationToken);
        stream.Position = 0;

        await fileService.UploadFileAsync($"setmissinghaseyps{clock.UtcNow:yyyyMMddHHmmss}.csv", stream, "text/csv");
    }
}
