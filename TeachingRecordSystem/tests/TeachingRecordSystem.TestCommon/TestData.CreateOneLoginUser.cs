using System.Text.Json;
using System.Text.Json.Nodes;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<OneLoginUser> CreateOneLoginUserAsync(CreatePersonResult createPersonResult, Option<string> subject = default, Option<string?> email = default) =>
        CreateOneLoginUserAsync(
            createPersonResult.PersonId,
            subject,
            email,
            verifiedInfo: ([createPersonResult.FirstName, createPersonResult.LastName], createPersonResult.DateOfBirth));

    public Task<OneLoginUser> CreateOneLoginUserAsync(Option<string> subject = default, Option<string?> email = default, bool verified = false) =>
        CreateOneLoginUserAsync(
            personId: null,
            subject,
            email,
            verifiedInfo: verified ? ([Faker.Name.First(), Faker.Name.Last()], DateOnly.FromDateTime(Faker.Identification.DateOfBirth())) : null);

    public Task<OneLoginUser> CreateOneLoginUserAsync(
        Guid? personId,
        Option<string> subject = default,
        Option<string?> email = default,
        (string[] Name, DateOnly DateOfBirth)? verifiedInfo = null)
    {
        if (personId is not null && verifiedInfo is null)
        {
            throw new ArgumentException("OneLoginUser with a Person must be verified.", nameof(verifiedInfo));
        }

        return WithDbContextAsync(async dbContext =>
        {
            var hasSignedInBefore = email != Option.Some((string?)null);

            var user = new OneLoginUser()
            {
                Subject = subject.ValueOr(CreateOneLoginUserSubject())
            };

            if (hasSignedInBefore)
            {
                user.EmailAddress = email.ValueOr(Faker.Internet.Email());
                user.FirstOneLoginSignIn = Clock.UtcNow;
                user.LastOneLoginSignIn = Clock.UtcNow;
            }

            if (verifiedInfo is not null)
            {
                user.SetVerified(
                    Clock.UtcNow,
                    OneLoginUserVerificationRoute.OneLogin,
                    verifiedByApplicationUserId: null,
                    [verifiedInfo!.Value.Name],
                    [verifiedInfo!.Value.DateOfBirth]);
            }

            if (personId is not null)
            {
                user.SetMatched(personId.Value, OneLoginUserMatchRoute.Automatic, matchedAttributes: null);

                if (hasSignedInBefore)
                {
                    user.FirstSignIn = Clock.UtcNow;
                    user.LastSignIn = Clock.UtcNow;
                }
            }

            dbContext.OneLoginUsers.Add(user);

            await dbContext.SaveChangesAsync();

            return user;
        });
    }

    public string CreateOneLoginUserSubject() => Guid.NewGuid().ToString("N");

    public JsonDocument CreateOneLoginCoreIdentityVc(string firstName, string lastName, DateOnly dateOfBirth) =>
        JsonDocument.Parse(
            new JsonObject
            {
                ["type"] = new JsonArray(
                    JsonValue.Create("VerifiableCredential"),
                    JsonValue.Create("IdentityCheckCredential")),
                ["aud"] = "test_client_id",
                ["credentialSubject"] = new JsonObject
                {
                    ["name"] = new JsonArray(
                        new JsonObject
                        {
                            ["nameParts"] = new JsonArray(
                                new JsonObject
                                {
                                    ["value"] = firstName,
                                    ["type"] = "GivenName"
                                },
                                new JsonObject
                                {
                                    ["value"] = lastName,
                                    ["type"] = "FamilyName"
                                })
                        }),
                    ["birthDate"] = new JsonArray(
                        new JsonObject
                        {
                            ["value"] = dateOfBirth.ToString("yyyy-MM-dd")
                        })
                }
            }.ToString());
}
