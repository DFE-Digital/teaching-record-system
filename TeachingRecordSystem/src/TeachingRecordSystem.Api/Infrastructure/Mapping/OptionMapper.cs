using OneOf;
using Optional;

namespace TeachingRecordSystem.Api.Infrastructure.Mapping;

public class OptionToOptionTypeConverter<TSource, TDestination> : ITypeConverter<Option<TSource>, Option<TDestination>>
{
    public Option<TDestination> Convert(Option<TSource> source, Option<TDestination> destination, ResolutionContext context) =>
        source.Map(source => context.Mapper.Map<TDestination>(source));
}

public class WrapWithOptionValueConverter<TSource, TDestination> : IValueConverter<TSource, Option<TDestination>>
{
    public Option<TDestination> Convert(TSource sourceMember, ResolutionContext context) =>
        Option.Some(context.Mapper.Map<TDestination>(sourceMember));
}

public class WrapWithOptionValueConverter<T> : IValueConverter<T, Option<T>>
{
    public Option<T> Convert(T sourceMember, ResolutionContext context) =>
        Option.Some(sourceMember);
}

public class OneOfToOneOfTypeConverter<T0Source, T1Source, T0Destination, T1Destination> : ITypeConverter<OneOf<T0Source, T1Source>, OneOf<T0Destination, T1Destination>>
{
    public OneOf<T0Destination, T1Destination> Convert(OneOf<T0Source, T1Source> source, OneOf<T0Destination, T1Destination> destination, ResolutionContext context)
    {
        return source.Match(
            v => OneOf<T0Destination, T1Destination>.FromT0(context.Mapper.Map<T0Destination>(v)),
            v => OneOf<T0Destination, T1Destination>.FromT1(context.Mapper.Map<T1Destination>(v)));
    }
}

public class FromOneOfT0TypeConverter<T0Source, T1Source, TDestination> : ITypeConverter<OneOf<T0Source, T1Source>, TDestination>
{
    public TDestination Convert(OneOf<T0Source, T1Source> source, TDestination destination, ResolutionContext context)
    {
        return context.Mapper.Map<TDestination>(source.AsT0);
    }
}
