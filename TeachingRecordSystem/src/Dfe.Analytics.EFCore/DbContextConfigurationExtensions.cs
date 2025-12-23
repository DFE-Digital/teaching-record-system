using Dfe.Analytics.EFCore.Description;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.Analytics.EFCore;

public static class DbContextConfigurationExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static T HasAnalyticsSync<T>(this T builder, bool syncTable = true) where T : EntityTypeBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasAnnotation(AnnotationKeys.TableAnalyticsSyncMetadata, new TableSyncMetadata(syncTable));

        return builder;
    }
}
