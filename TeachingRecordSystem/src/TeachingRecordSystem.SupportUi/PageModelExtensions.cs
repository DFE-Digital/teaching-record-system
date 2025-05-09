using System.Transactions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi;

public static class PageModelExtensions
{
    private static readonly TimeSpan _defaultSyncThreshold = TimeSpan.FromSeconds(5);

    public static PageResult PageWithErrors(this PageModel pageModel) => new PageResult() { StatusCode = StatusCodes.Status400BadRequest };

    public static async Task SyncPersonAsync(
        this PageModel pageModel,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var syncHelper = pageModel.HttpContext.RequestServices.GetRequiredService<TrsDataSyncHelper>();

        using var txn = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
        await syncHelper.SyncPersonAsync(personId, syncAudit: false, cancellationToken: cancellationToken);
    }

    public static async Task<bool> TrySyncPersonAsync(
        this PageModel pageModel,
        Guid personId,
        TimeSpan? threshold = null)
    {
        var logger = pageModel.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(PageModelExtensions));

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(threshold ?? _defaultSyncThreshold);

        try
        {
            await SyncPersonAsync(pageModel, personId, cts.Token);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed syncing person.");
            return false;
        }
    }
}
