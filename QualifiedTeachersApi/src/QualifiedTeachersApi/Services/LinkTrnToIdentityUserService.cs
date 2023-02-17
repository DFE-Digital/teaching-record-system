using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.Services.GetAnIdentityApi;

namespace QualifiedTeachersApi.Services
{
    public class LinkTrnToIdentityUserService : BackgroundService
    {
        private static readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);
        private readonly IDataverseAdapter _dataverseAdapter;
        private readonly ILogger<LinkTrnToIdentityUserService> _logger;
        private readonly IGetAnIdentityApiClient _identityApiClient;
        private readonly IServiceProvider _serviceProvider;

        public LinkTrnToIdentityUserService(IDataverseAdapter dataverse, ILogger<LinkTrnToIdentityUserService> logger, IServiceProvider serviceProvider, IGetAnIdentityApiClient client)
        {
            _dataverseAdapter = dataverse;
            _logger = logger;
            _identityApiClient = client;
            _serviceProvider = serviceProvider;
        }

        public async Task AssociateTrnsNotLinkedToIdentities()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dqtContext = scope.ServiceProvider.GetRequiredService<DqtContext>();
                var trnsNotLinkedToIndentities = await dqtContext.TrnRequests.Where(x => x.LinkedToIdentity == false && x.IdentityUserId.HasValue).ToListAsync();
                foreach (var trn in trnsNotLinkedToIndentities)
                {
                    var teacher = await _dataverseAdapter.GetTeacher(
                        trn.TeacherId.Value,
                        columnNames: new[]
                        {
                                    Contact.Fields.dfeta_TRN
                        });

                    if (teacher is not null)
                    {
                        try
                        {
                            //call api to link account to trn
                            await _identityApiClient.SetTeacherTrn(trn.IdentityUserId.Value, teacher.dfeta_TRN);
                            trn.LinkedToIdentity = true;
                            await dqtContext.SaveChangesAsync();

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error occurred while linking an identity {trn.IdentityUserId} to {teacher.dfeta_TRN}");
                        }
                    }
                    else
                    {
                        _logger.LogError($"{trn.TeacherId.Value} teacher not found!");
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
}
