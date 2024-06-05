using Optional;

namespace TeachingRecordSystem.Api.Infrastructure.Mapping;

public class OptionMapper<TSource, TDestination> : ITypeConverter<Option<TSource>, Option<TDestination>>
{
    public Option<TDestination> Convert(Option<TSource> source, Option<TDestination> destination, ResolutionContext context) =>
        source.Map(source => context.Mapper.Map<TDestination>(source));
}
