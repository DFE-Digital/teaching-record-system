using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Medallion.Threading;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Endpoints.IdentityWebHooks.Messages;
using TeachingRecordSystem.Api.Infrastructure.Json;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.Endpoints.IdentityWebHooks;

public static class GetAnIdentityEndpoints
{
    static GetAnIdentityEndpoints()
    {
        SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter(),
                new NotificationEnvelopeConverter()
            }
        };

        SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers =
            {
                Modifiers.OptionProperties
            }
        };
    }

    public static JsonSerializerOptions SerializerOptions { get; }

    private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    public static IEndpointConventionBuilder MapIdentityEndpoints(this IEndpointRouteBuilder builder)
    {
        return builder.MapGroup("/identity")
            .MapPost(
                "",
                async (
                    HttpContext context,
                    IOptions<GetAnIdentityOptions> identityOptions,
                    IDataverseAdapter dataverseAdapter,
                    IDistributedLockProvider distributedLockProvider
                ) =>
            {
                // Verify this was sent from identity
                if (!context.Request.Headers.TryGetValue("X-Hub-Signature-256", out var signature))
                {
                    return Results.Unauthorized();
                }

                using var sr = new StreamReader(context.Request.Body);
                var body = await sr.ReadToEndAsync();
                var secretBytes = Encoding.UTF8.GetBytes(identityOptions.Value.WebHookClientSecret);
                var bodyBytes = Encoding.UTF8.GetBytes(body);
                var calculatedSignature = Convert.ToHexString(HMACSHA256.HashData(secretBytes, bodyBytes));

                if (!calculatedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Unauthorized();
                }

                var notification = JsonSerializer.Deserialize<NotificationEnvelope>(body, SerializerOptions)!;

                User? user = null;

                if (notification.Message is UserUpdatedMessage userUpdatedMessage)
                {
                    user = userUpdatedMessage.User;
                }
                else if (notification.Message is UserCreatedMessage userCreatedMessage)
                {
                    user = userCreatedMessage.User;
                }
                else
                {
                    return CreateResult();
                }

                if (user.Trn is null)
                {
                    if (notification.Message is UserUpdatedMessage { Changes: { Trn: { HasValue: true } } })
                    {
                        // TRN has been removed
                        await dataverseAdapter.ClearTeacherIdentityInfo(user.UserId, notification.TimeUtc);
                    }

                    return CreateResult();
                }

                await using var trnLock = await distributedLockProvider.AcquireLockAsync(DistributedLockKeys.Trn(user.Trn), _lockTimeout);

                var teacher = await dataverseAdapter.GetTeacherByTrn(
                    user.Trn,
                    columnNames: new[]
                    {
                        Contact.Fields.dfeta_LastIdentityUpdate
                    });

                if (notification.TimeUtc > (teacher.dfeta_LastIdentityUpdate ?? DateTime.MinValue))
                {
                    await dataverseAdapter.UpdateTeacherIdentityInfo(new UpdateTeacherIdentityInfoCommand()
                    {
                        TeacherId = teacher.Id,
                        IdentityUserId = user.UserId,
                        EmailAddress = user.EmailAddress,
                        MobilePhone = user.MobileNumber,
                        UpdateTimeUtc = notification.TimeUtc
                    });
                }

                return CreateResult();

                static IResult CreateResult() => Results.NoContent();
            });
    }
}
