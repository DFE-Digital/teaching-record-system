using System.Text.Json;
using System.Text.Json.Nodes;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<OneLoginUser> CreateOneLoginUser(
        Guid? personId,
        string? subject = null,
        string? email = null)
    {
        return WithDbContext(async dbContext =>
        {
            subject ??= CreateOneLoginUserSubject();
            email ??= Faker.Internet.Email();

            var user = new OneLoginUser()
            {
                Subject = subject,
                Email = email,
                FirstOneLoginSignIn = Clock.UtcNow,
                LastOneLoginSignIn = Clock.UtcNow,
                PersonId = personId
            };

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
