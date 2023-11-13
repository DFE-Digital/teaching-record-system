using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure.FakeXrmEasy.Plugins;

/// <summary>
/// This plugin mirrors the poorly named QTSRegistrationDeletPlugin in CRM.
/// </summary>
public class QtsRegistrationUpdatedPlugin : IPlugin
{
    internal static void Register(IXrmFakedContext context)
    {
        var preImageDefinition = new PluginImageDefinition(
            "preImage",
            ProcessingStepImageType.PreImage,
            new string[]
            {
                dfeta_qtsregistration.Fields.dfeta_PersonId,
                dfeta_qtsregistration.Fields.dfeta_QTSDate,
                dfeta_qtsregistration.Fields.dfeta_EYTSDate
            });
        context.RegisterPluginStep<QtsRegistrationUpdatedPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = dfeta_qtsregistration.EntityLogicalName,
                MessageName = "Delete",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { preImageDefinition }
            });
        context.RegisterPluginStep<QtsRegistrationUpdatedPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = dfeta_qtsregistration.EntityLogicalName,
                MessageName = "Update",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { preImageDefinition }
            });
    }

    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IPluginExecutionContext>();
        var orgService = serviceProvider.GetRequiredService<IOrganizationService>();

        if (context.PreEntityImages.TryGetValue("preImage", out var preImage))
        {
            if (preImage.TryGetAttributeValue<EntityReference>(dfeta_qtsregistration.Fields.dfeta_PersonId, out var personIdEntityReference))
            {
                var personId = personIdEntityReference.Id;
                if (context.MessageName == "Delete")
                {
                    UpdatePersonQtsDate(orgService, personId, null);
                    UpdatePersonEytsDate(orgService, personId, null);
                }
                else
                {
                    var target = context.InputParameters["Target"] as Entity;
                    var state = target!.GetAttributeValue<OptionSetValue>(dfeta_qtsregistration.Fields.StateCode);
                    if (state?.Value == 1)
                    {
                        UpdatePersonQtsDate(orgService, personId, null);
                        UpdatePersonEytsDate(orgService, personId, null);
                    }
                    else
                    {
                        var qtsDate = target.GetAttributeValue<DateTime?>(dfeta_qtsregistration.Fields.dfeta_QTSDate);
                        var eytsDate = target.GetAttributeValue<DateTime?>(dfeta_qtsregistration.Fields.dfeta_EYTSDate);

                        if (qtsDate.HasValue)
                        {
                            UpdatePersonQtsDate(orgService, personId, qtsDate);
                        }

                        if (eytsDate.HasValue)
                        {
                            UpdatePersonEytsDate(orgService, personId, eytsDate);
                        }
                    }
                }
            }
        }
    }

    private void UpdatePersonQtsDate(IOrganizationService orgService, Guid personId, DateTime? qtsDate)
    {
        orgService.Execute(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = personId,
                dfeta_QTSDate = qtsDate
            }
        });
    }

    private void UpdatePersonEytsDate(IOrganizationService orgService, Guid personId, DateTime? eytsDate)
    {
        orgService.Execute(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = personId,
                dfeta_EYTSDate = eytsDate
            }
        });
    }
}
