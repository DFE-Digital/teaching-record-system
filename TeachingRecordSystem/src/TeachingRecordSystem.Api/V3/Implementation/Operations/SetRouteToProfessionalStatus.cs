using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetRouteToProfessionalStatusCommand(
    string Trn,
    string SourceApplicationReference,
    Guid RouteToProfessionalStatusTypeId,
    RouteToProfessionalStatusStatus Status,
    DateOnly? HoldsFrom,
    DateOnly? TrainingStartDate,
    DateOnly? TrainingEndDate,
    string[] TrainingSubjectReferences,
    SetRouteToProfessionalStatusCommandTrainingAgeSpecialism? TrainingAgeSpecialism,
    string? TrainingCountryReference,
    string? TrainingProviderUkprn,
    Guid? DegreeTypeId,
    bool? IsExemptFromInduction) :
    ICommand<SetRouteToProfessionalStatusResult>;

public record SetRouteToProfessionalStatusCommandTrainingAgeSpecialism(
    TrainingAgeSpecialismType Type,
    int? From,
    int? To);

public record SetRouteToProfessionalStatusResult;

public class SetRouteToProfessionalStatusHandler(
    TrsDbContext dbContext,
    IEventPublisher eventPublisher,
    ICurrentUserProvider currentUserProvider,
    ReferenceDataCache referenceDataCache,
    IClock clock) :
    ICommandHandler<SetRouteToProfessionalStatusCommand, SetRouteToProfessionalStatusResult>
{
    private static readonly IReadOnlyCollection<Guid> _permittedRouteTypeIds =
    [
        RouteToProfessionalStatusType.ApplyForQtsId,
        RouteToProfessionalStatusType.EuropeanRecognitionId,
        RouteToProfessionalStatusType.OverseasTrainedTeacherRecognitionId,
        RouteToProfessionalStatusType.NiRId,
        RouteToProfessionalStatusType.ScotlandRId,
        new("6987240E-966E-485F-B300-23B54937FB3A"),
        new("57B86CEF-98E2-4962-A74A-D47C7A34B838"),
        new("4163C2FB-6163-409F-85FD-56E7C70A54DD"),
        new("4BD7A9F0-28CA-4977-A044-A7B7828D469B"),
        new("D9EEF3F8-FDE6-4A3F-A361-F6655A42FA1E"),
        new("4477E45D-C531-4C63-9F4B-E157766366FB"),
        new("DBC4125B-9235-41E4-ABD2-BAABBF63F829"),
        new("7F09002C-5DAD-4839-9693-5E030D037AE9"),
        new("C97C0FD2-FD84-4949-97C7-B0E2422FB3C8"),
        new("F85962C9-CF0C-415D-9DE5-A397F95AE261"),
        new("34222549-ED59-4C4A-811D-C0894E78D4C3"),
        new("10078157-E8C3-42F7-A050-D8B802E83F7B"),
        new("BFEF20B2-5AC4-486D-9493-E5A4538E1BE9"),
        new("D0B60864-AB1C-4D49-A5C2-FF4BD9872EE1"),
        new("2B4862CA-BD30-4A3A-BFCE-52B57C2946C7"),
        new("51756384-CFEA-4F63-80E5-F193686E0F71"),
        new("EF46FF51-8DC0-481E-B158-61CCEA9943D9"),
        new("321D5F9A-9581-4936-9F63-CFDDD2A95FE2"),
        new("97497716-5AC5-49AA-A444-27FA3E2C152A"),
        new("53A7FBDA-25FD-4482-9881-5CF65053888D"),
        new("70368FF2-8D2B-467E-AD23-EFE7F79995D7"),
        new("D9490E58-ACDC-4A38-B13E-5A5C21417737"),
        new("12A742C3-1CD4-43B7-A2FA-1000BD4CC373"),
        new("97E1811B-D46C-483E-AEC3-4A2DD51A55FE"),
        new("5B7F5E90-1CA6-4529-BAA0-DFBA68E698B8"),
        new("20F67E38-F117-4B42-BBFC-5812AA717B94")
    ];

    public async Task<ApiResult<SetRouteToProfessionalStatusResult>> ExecuteAsync(SetRouteToProfessionalStatusCommand command)
    {
        var person = await dbContext.Persons
            .Where(p => p.Trn == command.Trn)
            .Include(p => p.Qualifications).AsSplitQuery()
            .SingleOrDefaultAsync();

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        if (!_permittedRouteTypeIds.Contains(command.RouteToProfessionalStatusTypeId))
        {
            return ApiError.InvalidRouteType(command.RouteToProfessionalStatusTypeId);
        }

        var allTrainingSubjects = await referenceDataCache.GetTrainingSubjectsAsync(activeOnly: true);
        List<Guid> subjectIds = new();
        foreach (var subjectReference in command.TrainingSubjectReferences)
        {
            var subject = allTrainingSubjects.SingleOrDefault(s => s.Reference == subjectReference);

            if (subject is null)
            {
                return ApiError.InvalidTrainingSubjectReference(subjectReference);
            }

            subjectIds.Add(subject.TrainingSubjectId);
        }

        if (command.TrainingCountryReference is not null)
        {
            var country = (await referenceDataCache.GetTrainingCountriesAsync())
                .SingleOrDefault(c => c.CountryId == command.TrainingCountryReference);

            if (country is null)
            {
                return ApiError.InvalidTrainingCountryReference(command.TrainingCountryReference);
            }
        }

        Guid? trainingProviderId = null;
        if (command.TrainingProviderUkprn is not null)
        {
            var trainingProvider = (await referenceDataCache.GetTrainingProvidersAsync())
                .SingleOrDefault(p => p.Ukprn == command.TrainingProviderUkprn);

            if (trainingProvider is null)
            {
                return ApiError.InvalidTrainingProviderUkprn(command.TrainingProviderUkprn);
            }

            trainingProviderId = trainingProvider.TrainingProviderId;
        }
        else if (command.RouteToProfessionalStatusTypeId.IsOverseas())
        {
            trainingProviderId = await GetTrainingProviderIdForOverseasRouteAsync(command.RouteToProfessionalStatusTypeId);
        }

        Guid? degreeTypeId = null;
        if (command.DegreeTypeId is not null)
        {
            var degreeType = (await referenceDataCache.GetDegreeTypesAsync())
                .SingleOrDefault(d => d.DegreeTypeId == command.DegreeTypeId);

            if (degreeType is null)
            {
                return ApiError.InvalidDegreeType(command.DegreeTypeId.Value);
            }

            degreeTypeId = degreeType.DegreeTypeId;
        }

        var (currentUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var route = await dbContext.RouteToProfessionalStatuses.SingleOrDefaultAsync(r =>
            r.PersonId == person.PersonId &&
            r.SourceApplicationReference == command.SourceApplicationReference &&
            r.SourceApplicationUserId == currentUserId);

        if (route is not null)
        {
            // No current provision for changing overseas
            if (command.RouteToProfessionalStatusTypeId.IsOverseas())
            {
                return ApiError.UpdatesNotAllowedForRouteType(command.RouteToProfessionalStatusTypeId);
            }

            // Can't change between Early Years and non-Early Years
            var routeType = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(command.RouteToProfessionalStatusTypeId);
            var existingRouteType = route.RouteToProfessionalStatusType!;
            if (routeType.ProfessionalStatusType != existingRouteType.ProfessionalStatusType)
            {
                return ApiError.UnableToChangeRouteType();
            }

            switch (route.Status)
            {
                // If the route has already been Awarded then this can't be changed via the API - needs to be altered via TRS console
                case RouteToProfessionalStatusStatus.Holds:
                    return ApiError.RouteToProfessionalStatusAlreadyAwarded();
                case RouteToProfessionalStatusStatus.Failed:
                    switch (command.Status)
                    {
                        case RouteToProfessionalStatusStatus.Deferred:
                        case RouteToProfessionalStatusStatus.InTraining:
                        case RouteToProfessionalStatusStatus.UnderAssessment:
                            return ApiError.UnableToChangeFailProfessionalStatusStatus();
                    }
                    break;
                case RouteToProfessionalStatusStatus.Withdrawn:
                    if (command.Status == RouteToProfessionalStatusStatus.Deferred)
                    {
                        return ApiError.UnableToChangeWithdrawnProfessionalStatusStatus();
                    }
                    break;
            }

            route.Update(
                allRouteTypes: await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(),
                r =>
                {
                    r.RouteToProfessionalStatusTypeId = command.RouteToProfessionalStatusTypeId;
                    r.Status = command.Status;
                    r.HoldsFrom = command.HoldsFrom;
                    r.TrainingStartDate = command.TrainingStartDate;
                    r.TrainingEndDate = command.TrainingEndDate;
                    r.TrainingSubjectIds = subjectIds.ToArray();
                    r.TrainingAgeSpecialismType = command.TrainingAgeSpecialism?.Type;
                    r.TrainingAgeSpecialismRangeFrom = command.TrainingAgeSpecialism?.From;
                    r.TrainingAgeSpecialismRangeTo = command.TrainingAgeSpecialism?.To;
                    r.TrainingProviderId = trainingProviderId;
                    r.DegreeTypeId = degreeTypeId;
                    r.ExemptFromInduction = command.IsExemptFromInduction;
                },
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                currentUserId,
                clock.UtcNow,
                out var @event);

            await dbContext.SaveChangesAsync();

            if (@event is not null)
            {
                await eventPublisher.PublishEventAsync(@event);
            }
        }
        else
        {
            route = RouteToProfessionalStatus.Create(
                person,
                allRouteTypes: await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(),
                routeToProfessionalStatusTypeId: command.RouteToProfessionalStatusTypeId,
                sourceApplicationUserId: currentUserId,
                sourceApplicationReference: command.SourceApplicationReference,
                status: command.Status,
                holdsFrom: command.HoldsFrom,
                trainingStartDate: command.TrainingStartDate,
                trainingEndDate: command.TrainingEndDate,
                trainingSubjectIds: subjectIds.ToArray(),
                trainingAgeSpecialismType: command.TrainingAgeSpecialism?.Type,
                trainingAgeSpecialismRangeFrom: command.TrainingAgeSpecialism?.From,
                trainingAgeSpecialismRangeTo: command.TrainingAgeSpecialism?.To,
                trainingCountryId: command.TrainingCountryReference,
                trainingProviderId: trainingProviderId,
                degreeTypeId: degreeTypeId,
                isExemptFromInduction: command.IsExemptFromInduction,
                createdBy: currentUserId,
                now: clock.UtcNow,
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                @event: out var @event);

            dbContext.RouteToProfessionalStatuses.Add(route);

            await dbContext.SaveChangesAsync();

            await eventPublisher.PublishEventAsync(@event);
        }

        return new SetRouteToProfessionalStatusResult();
    }

    private async Task<Guid> GetTrainingProviderIdForOverseasRouteAsync(Guid routeToProfessionalStatusTypeId)
    {
        var ittProviderName =
            routeToProfessionalStatusTypeId == RouteToProfessionalStatusType.ApplyForQtsId ? "Non-UK establishment" : // Apply for QTS
            routeToProfessionalStatusTypeId == RouteToProfessionalStatusType.EuropeanRecognitionId ? "Non-UK establishment" : // European Recognition
            routeToProfessionalStatusTypeId == RouteToProfessionalStatusType.NiRId ? "UK establishment (Scotland/Northern Ireland)" : // NI R
            routeToProfessionalStatusTypeId == RouteToProfessionalStatusType.OverseasTrainedTeacherRecognitionId ? "Non-UK establishment" :  // Overseas Trained Teacher Recognition
            routeToProfessionalStatusTypeId == RouteToProfessionalStatusType.ScotlandRId ? "UK establishment (Scotland/Northern Ireland)" : // Scotland R
            throw new ArgumentException($"Unknown route type ID: '{routeToProfessionalStatusTypeId}'.", nameof(routeToProfessionalStatusTypeId));

        var ittProvider = (await referenceDataCache.GetTrainingProvidersAsync(activeOnly: false))
            .Single(p => p.Name == ittProviderName);

        return ittProvider.TrainingProviderId;
    }
}
