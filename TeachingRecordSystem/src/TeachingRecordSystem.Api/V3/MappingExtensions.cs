using OneOf;
using Optional;

namespace TeachingRecordSystem.Api.V3;

internal static class MappingExtensions
{
    public static Option<IReadOnlyCollection<TDest>> MapItems<TSource, TDest>(
        this Option<IReadOnlyCollection<TSource>> source,
        Func<TSource, TDest> selector) =>
        source.Map(items => (IReadOnlyCollection<TDest>)items.Select(selector).ToArray());

    public static TDest FromT0<T0, T1, TDest>(
        this OneOf<T0, T1> source,
        Func<T0, TDest> converter) =>
        converter(source.AsT0);
}
