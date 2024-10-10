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

