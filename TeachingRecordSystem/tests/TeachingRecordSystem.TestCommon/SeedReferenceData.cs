using System.Diagnostics;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public class SeedReferenceData : IStartupTask
{
    private readonly IOrganizationServiceAsync _organizationServiceAsync;

    public SeedReferenceData(IOrganizationServiceAsync organizationServiceAsync)
    {
        _organizationServiceAsync = organizationServiceAsync;
        Debug.Assert(organizationServiceAsync is FakeServiceClient);
    }

    public Task Execute()
    {
        AddSubjects();

        return Task.CompletedTask;
    }

    private void AddSubjects()
    {
        _organizationServiceAsync.Create(new Subject()
        {
            Title = "Change of Date of Birth"
        });

        _organizationServiceAsync.Create(new Subject()
        {
            Title = "Change of Name"
        });
    }
}
