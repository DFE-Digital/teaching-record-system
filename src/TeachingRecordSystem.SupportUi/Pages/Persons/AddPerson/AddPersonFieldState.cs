namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public class AddPersonFieldState<T>(string raw, T? parsed) where T : class, IParsable<T>
{
    public string Raw { get; } = raw;
    public T? Parsed { get; } = parsed;

#pragma warning disable CA1000
    public static AddPersonFieldState<T> FromRawValue(string? rawValue)
#pragma warning restore CA1000
    {
        return new(rawValue ?? "", T.TryParse(rawValue, null, out var parsed) ? (T?)parsed : null);
    }

    public override bool Equals(object? obj)
    {
        return obj is AddPersonFieldState<T> state &&
            (Parsed != null && state.Parsed != null && Parsed.Equals(state.Parsed)
            || Raw == state.Raw);
    }

    public override int GetHashCode()
    {
        return Parsed != null ? HashCode.Combine(Parsed) : HashCode.Combine(Raw);
    }

    public static bool operator ==(AddPersonFieldState<T>? left, AddPersonFieldState<T>? right) =>
        left is null && right is null ||
        left is not null && right is not null && left.Equals(right);

    public static bool operator !=(AddPersonFieldState<T>? left, AddPersonFieldState<T>? right) =>
        !(left == right);
}
