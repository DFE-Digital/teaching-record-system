﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using QualifiedTeachersApi.DataStore.Sql;

namespace QualifiedTeachersApi.Services.CrmEntityChanges;

public class CrmEntityChangesService : ICrmEntityChangesService
{
    private readonly IDbContextFactory<DqtContext> _dbContextFactory;
    private readonly IOrganizationServiceAsync _organizationService;

    public CrmEntityChangesService(IDbContextFactory<DqtContext> dbContextFactory, IOrganizationServiceAsync organizationService)
    {
        _dbContextFactory = dbContextFactory;
        _organizationService = organizationService;
    }

    public async IAsyncEnumerable<IChangedItem[]> GetEntityChanges(
        string key,
        string entityLogicalName,
        int pageSize = 1000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entityChangesJournal = await dbContext.EntityChangesJournals
            .SingleOrDefaultAsync(t => t.Key == key && t.EntityLogicalName == entityLogicalName);

        var request = new RetrieveEntityChangesRequest()
        {
            Columns = new Microsoft.Xrm.Sdk.Query.ColumnSet(allColumns: true),
            EntityName = entityLogicalName,
            PageInfo = new()
            {
                Count = pageSize,
                PageNumber = 1
            },
            DataVersion = entityChangesJournal?.DataToken
        };

        var gotData = false;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = (RetrieveEntityChangesResponse)await _organizationService.ExecuteAsync(request);

            gotData &= response.EntityChanges.Changes.Count > 0;

            yield return response.EntityChanges.Changes.ToArray();

            if (!response.EntityChanges.MoreRecords)
            {
                if (gotData)
                {
                    if (entityChangesJournal is not null)
                    {
                        entityChangesJournal.DataToken = response.EntityChanges.DataToken;
                    }
                    else
                    {
                        dbContext.EntityChangesJournals.Add(new()
                        {
                            Key = key,
                            EntityLogicalName = entityLogicalName,
                            DataToken = response.EntityChanges.DataToken
                        });
                    }

                    await dbContext.SaveChangesAsync();
                }

                break;
            }

            request.PageInfo.PageNumber++;
            request.PageInfo.PagingCookie = response.EntityChanges.PagingCookie;
        }
    }
}
