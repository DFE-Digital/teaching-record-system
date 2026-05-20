namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public class EditDetailsFieldState<T>(string raw, T? parsed) where T : class, IParsable<T>
{
    public string Raw { get; } = raw;
    public T? Parsed { get; } = parsed;

#pragma warning disable CA1000
    public static EditDetailsFieldState<T> FromRawValue(string? rawValue)
#pragma warning restore CA1000
    {
        return new(rawValue ?? "", T.TryParse(rawValue, null, out var parsed) ? (T?)parsed : null);
    }

    public override bool Equals(object? obj)
    {
        return obj is EditDetailsFieldState<T> state &&
            (Parsed != null && state.Parsed != null && Parsed.Equals(state.Parsed)
            || Raw == state.Raw);
    }

    public override int GetHashCode()
    {
        return Parsed != null ? HashCode.Combine(Parsed) : HashCode.Combine(Raw);
    }

    public static bool operator ==(EditDetailsFieldState<T>? left, EditDetailsFieldState<T>? right) =>
        (left is null && right is null) ||
        (left is not null && right is not null && left.Equals(right));

    public static bool operator !=(EditDetailsFieldState<T>? left, EditDetailsFieldState<T>? right) =>
        !(left == right);
}
