using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core.Events.Models;

/// <summary>
/// Represents the user who raised the event.
/// Contains either a TRS user ID or the DQT user ID and name.
/// </summary>
[JsonConverter(typeof(RaisedByUserInfoJsonConverter))]
public sealed class RaisedByUserInfo
{
    private RaisedByUserInfo() { }

    public Guid? UserId { get; private set; }

    public Guid? DqtUserId { get; private set; }
    public string? DqtUserName { get; private set; }

    [MemberNotNullWhen(true, nameof(DqtUserId), nameof(DqtUserName))]
    [MemberNotNullWhen(false, nameof(UserId))]
    public bool IsDqtUser => DqtUserId.HasValue;

    public static implicit operator RaisedByUserInfo(Guid userId) => FromUserId(userId);

    public static RaisedByUserInfo FromUserId(Guid userId) => new()
    {
        UserId = userId
    };

    public static RaisedByUserInfo FromDqtUser(Guid dqtUserId, string dqtUserName) => new()
    {
        DqtUserId = dqtUserId,
        DqtUserName = dqtUserName
    };
}

public class RaisedByUserInfoJsonConverter : JsonConverter<RaisedByUserInfo>
{
    public override RaisedByUserInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var dqtUserInfo = JsonSerializer.Deserialize<DqtUserInfo>(ref reader, options)!;
            return RaisedByUserInfo.FromDqtUser(dqtUserInfo.DqtUserId, dqtUserInfo.DqtUserName);
        }
        else
        {
            var userId = reader.GetGuid();
            return RaisedByUserInfo.FromUserId(userId);
        }
    }

    public override void Write(Utf8JsonWriter writer, RaisedByUserInfo value, JsonSerializerOptions options)
    {
        if (value.IsDqtUser)
        {
            var dqtUserInfo = new DqtUserInfo()
            {
                DqtUserId = value.DqtUserId.Value,
                DqtUserName = value.DqtUserName
            };
            JsonSerializer.Serialize(writer, dqtUserInfo, options);
        }
        else
        {
            writer.WriteStringValue(value.UserId.ToString());
        }
    }

    private sealed class DqtUserInfo
    {
        public required Guid DqtUserId { get; init; }
        public required string DqtUserName { get; init; }
    }
}
