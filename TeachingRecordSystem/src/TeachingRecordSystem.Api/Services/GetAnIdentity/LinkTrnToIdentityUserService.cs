using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using TeachingRecordSystem.Api.DataStore.Sql;
using TeachingRecordSystem.Api.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.Services;

public class LinkTrnToIdentityUserService : BackgroundService
{
    private static readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);
    private readonly ILogger<LinkTrnToIdentityUserService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public LinkTrnToIdentityUserService(IServiceProvider serviceProvider, ILogger<LinkTrnToIdentityUserService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task AssociateTrnsNotLinkedToIdentities()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dqtContext = scope.ServiceProvider.GetRequiredService<DqtContext>();
            var dataverseAdapter = scope.ServiceProvider.GetRequiredService<IDataverseAdapter>();
            var identityApiClient = scope.ServiceProvider.GetRequiredService<IGetAnIdentityApiClient>();

            var trnsNotLinkedToIndentities = await dqtContext.TrnRequests.Where(x => x.LinkedToIdentity == false && x.IdentityUserId.HasValue).ToListAsync();

            foreach (var trnRequest in trnsNotLinkedToIndentities)
            {
                var teacher = await dataverseAdapter.GetTeacher(
                    trnRequest.TeacherId,
                    columnNames: new[]
                    {
                        Contact.Fields.dfeta_TRN
                    });

                if (teacher is not null)
                {
                    try
                    {
                        //call api to link account to trn
                        await identityApiClient.SetTeacherTrn(trnRequest.IdentityUserId!.Value, teacher.dfeta_TRN);
                        trnRequest.LinkedToIdentity = true;
                        await dqtContext.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error occurred while linking an identity {trnRequest.IdentityUserId} to {teacher.dfeta_TRN}");
                    }
                }
                else
                {
                    _logger.LogError($"{trnRequest.TeacherId} teacher not found!");
                }
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(_pollInterval);
        do
        {
            try
            {
                await AssociateTrnsNotLinkedToIdentities();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed linking DQT contacts to Identity users.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
