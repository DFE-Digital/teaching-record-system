using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using QualifiedTeachersApi.Infrastructure.Json;
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

                if (!calculatedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Unauthorized();
                }

                NotificationEnvelope notification;
                try
                {
                    notification = JsonSerializer.Deserialize<NotificationEnvelope>(body, SerializerOptions)!;
                }
                catch (Exception)
                {
                    return Results.BadRequest();
                }

                if (notification.Message is UserUpdatedMessage)
                {                    
                    var userUpdatedMessage = (UserUpdatedMessage) notification.Message;
                    var request = new UserUpdatedRequest()
                    {
                        UserId = userUpdatedMessage.User.UserId,
                        Trn = userUpdatedMessage.User.Trn,
                        EmailAddress = userUpdatedMessage.User.EmailAddress,
                        MobileNumber = userUpdatedMessage.User.MobileNumber,
                        UpdateTimeUtc = notification.TimeUtc
                    };

                    await mediator.Send(request);
                }
                else if (notification.Message is UserCreatedMessage)
                {
                    var userCreatedMessage = (UserCreatedMessage)notification.Message;
                    var request = new UserCreatedRequest()
                    {
                        UserId = userCreatedMessage.User.UserId,
                        Trn = userCreatedMessage.User.Trn,
                        EmailAddress = userCreatedMessage.User.EmailAddress,
                        MobileNumber = userCreatedMessage.User.MobileNumber,
                        UpdateTimeUtc = notification.TimeUtc
                    };

                    await mediator.Send(request);
                }

                return Results.NoContent();
            });
    }

    static JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddConverters();
}
