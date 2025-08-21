using System.Diagnostics;
using MediatR;
using MoreLinq;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class FindTeachersHandler(TrsDbContext dbContext) : IRequestHandler<FindTeachersRequest, FindTeachersResponse>
{
    public async Task<FindTeachersResponse> Handle(FindTeachersRequest request, CancellationToken cancellationToken)
    {
        var result = request.MatchPolicy.GetValueOrDefault() == FindTeachersMatchPolicy.Default ?
            await HandleDefaultRequestAsync() :
            await HandleStrictRequestAsync();

        return new FindTeachersResponse()
        {
            Results = result.Select(p => new FindTeacherResult()
            {
                Trn = p.Trn!,
                EmailAddresses = !string.IsNullOrEmpty(p.EmailAddress) ? [p.EmailAddress] : [],
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                NationalInsuranceNumber = p.NationalInsuranceNumber,
                Uid = p.PersonId.ToString(),
                HasActiveSanctions = p.Alerts!.Any(a => a.IsOpen)
            })
        };

        async Task<IEnumerable<PostgresModels.Person>> HandleDefaultRequestAsync()
        {
            Debug.Assert(request.MatchPolicy.GetValueOrDefault() == FindTeachersMatchPolicy.Default);

            var firstNames = new[] { request.FirstName, request.PreviousFirstName }.ExceptEmpty();
            var lastNames = new[] { request.LastName, request.PreviousLastName }.ExceptEmpty();
            var names = firstNames.Cartesian(lastNames, (f, l) => $"{f} {l}").ToArray();

            return await dbContext.Persons.FromSqlRaw(
                $"""
                 WITH matches AS (
                    SELECT person_id FROM person_search_attributes
                    WHERE
                        (attribute_type IN ('FullName', 'PreviousFullName') AND attribute_value = ANY((:names collate "case_insensitive"))) OR
                        (attribute_type = 'DateOfBirth' AND attribute_value = (:date_of_birth collate "case_insensitive")) OR
                        (attribute_type = 'EmailAddress' AND attribute_value = (:email_address collate "case_insensitive")) OR
                        (attribute_type = 'Trn' AND attribute_value = (:trn collate "case_insensitive")) OR
                        (attribute_type = 'NationalInsuranceNumber' AND attribute_value = (:national_insurance_number collate "case_insensitive"))
                    GROUP BY person_id
                    HAVING COUNT(DISTINCT CASE WHEN attribute_type IN ('FullName', 'PreviousFullName') THEN 'Name' ELSE attribute_type END) >= 3
                 )
                 SELECT p.* FROM matches m
                 JOIN persons p ON p.person_id = m.person_id
                 """,
                parameters: [
                    new NpgsqlParameter("names", NpgsqlDbType.Varchar | NpgsqlDbType.Array) { Value = names.ToArray() },
                    new NpgsqlParameter("date_of_birth", NpgsqlDbType.Varchar) { Value = (object?)request.DateOfBirth?.ToString("yyyy-MM-dd") ?? DBNull.Value },
                    new NpgsqlParameter("email_address", NpgsqlDbType.Varchar) { Value = (object?)request.EmailAddress ?? DBNull.Value },
                    new NpgsqlParameter("trn", NpgsqlDbType.Varchar) { Value = (object?)request.Trn ?? DBNull.Value },
                    new NpgsqlParameter("national_insurance_number", NpgsqlDbType.Varchar) { Value = (object?)request.NationalInsuranceNumber ?? DBNull.Value }
                ])
                .ApplyBaseFilters()
                .ToArrayAsync();
        }

        async Task<IEnumerable<PostgresModels.Person>> HandleStrictRequestAsync()
        {
            // Match on DOB, NINO & TRN *OR*
            // DOB, TRN & Name & contact.NINO is null

            Debug.Assert(request.MatchPolicy == FindTeachersMatchPolicy.Strict);

            var normalizedNino = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

            return await dbContext.Persons
                .ApplyBaseFilters()
                .Where(p =>
                    p.DateOfBirth == request.DateOfBirth &&
                    p.Trn == request.Trn && (
                        (p.NationalInsuranceNumber != null && p.NationalInsuranceNumber == normalizedNino) ||
                        (p.NationalInsuranceNumber == null && p.FirstName == request.FirstName && p.LastName == request.LastName)))
                .ToArrayAsync();
        }
    }
}

file static class Extensions
{
    public static IQueryable<PostgresModels.Person> ApplyBaseFilters(this IQueryable<PostgresModels.Person> query) =>
        query.Include(p => p.Alerts).Where(p => p.Trn != null && p.Status == PersonStatus.Active);

    public static IEnumerable<string> ExceptEmpty(this IEnumerable<string> source) =>
        source.Where(v => !string.IsNullOrEmpty(v));
}
