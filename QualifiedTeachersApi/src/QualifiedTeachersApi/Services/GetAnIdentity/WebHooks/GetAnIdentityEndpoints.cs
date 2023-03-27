using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using QualifiedTeachersApi.Services.GetAnIdentityApi;

namespace QualifiedTeachersApi.Services.GetAnIdentity.WebHooks;

public static class GetAnIdentityEndpoints
{
    public static IEndpointConventionBuilder MapIdentityEndpoints(this IEndpointRouteBuilder builder)
    {
        return builder.MapGroup("/identity")
            .MapPost("", async (HttpContext context, IOptions<GetAnIdentityOptions> identityOptions, IMediator mediator) =>
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

                if (!calculatedSignature.Equals(signature))
                {
                    return Results.Unauthorized();
                }

                var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

                NotificationEnvelope notification = null;
                try
                {
                    notification = JsonSerializer.Deserialize<NotificationEnvelope>(body, serializerOptions);
                }
                catch (Exception)
                {
                    return Results.BadRequest();
                }

                if (notification.MessageType == UserUpdatedMessage.MessageTypeName)
                {
                    var jsonPayload = notification.Message as JsonElement?;
                    if (jsonPayload == null)
                    {
                        return Results.BadRequest();
                    }

                    UserUpdatedMessage userUpdatedMessage = null;

                    try
                    {
                        userUpdatedMessage = JsonSerializer.Deserialize<UserUpdatedMessage>(jsonPayload.Value, serializerOptions);
                    }
                    catch (Exception)
                    {
                        return Results.BadRequest();
                    }

                    var request = new UserUpdatedRequest()
                    {
                        UserId = userUpdatedMessage.User.UserId,
                        Trn = userUpdatedMessage.User.Trn,
                        EmailAddress = userUpdatedMessage.User.EmailAddress,
                        MobileNumber = userUpdatedMessage.User.MobileNumber,
                        UpdateTimeUtc = notification.TimeUtc
                    };

                    _ = await mediator.Send(request);
                }

                return Results.NoContent();
            });
    }
}
