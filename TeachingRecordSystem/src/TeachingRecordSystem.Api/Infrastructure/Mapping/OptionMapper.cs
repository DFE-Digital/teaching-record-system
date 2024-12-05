using Optional;
using Optional.Unsafe;

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

public class UnwrapFromOptionValueConverter<TSource, TDestination> : IValueConverter<Option<TSource>, TDestination>
{
    public TDestination Convert(Option<TSource> sourceMember, ResolutionContext context) =>
        context.Mapper.Map<TDestination>(sourceMember.ValueOrFailure());
}

public class UnwrapFromOptionValueConverter<T> : IValueConverter<Option<T>, T>
{
    public T Convert(Option<T> sourceMember, ResolutionContext context) =>
        sourceMember.ValueOrFailure();
}
