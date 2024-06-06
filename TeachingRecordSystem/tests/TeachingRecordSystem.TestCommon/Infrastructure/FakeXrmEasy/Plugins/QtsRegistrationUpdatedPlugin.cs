using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
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

        var preImageContact = new PluginImageDefinition(
            "PostContactImage",
            ProcessingStepImageType.PostImage,
            new string[]
            {
                Contact.Fields.ContactId,
                Contact.Fields.dfeta_qtlsdate
            });

        context.RegisterPluginStep<QtsRegistrationUpdatedPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = dfeta_qtsregistration.EntityLogicalName,
                MessageName = "Delete",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { preImageDefinition },
                Rank = 1
            });
        context.RegisterPluginStep<QtsRegistrationUpdatedPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = dfeta_qtsregistration.EntityLogicalName,
                MessageName = "Update",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { preImageDefinition },
                Rank = 5,
                FilteringAttributes = new string[]
                {
                    dfeta_qtsregistration.Fields.dfeta_PersonId,
                    dfeta_qtsregistration.Fields.dfeta_QTSDate,
                    dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                }
            });
        context.RegisterPluginStep<QtsRegistrationUpdatedPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = Contact.EntityLogicalName,
                MessageName = "Update",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { preImageContact },
                FilteringAttributes = new string[]
                {
                    Contact.Fields.dfeta_qtlsdate
                }
            });

        context.RegisterPluginStep<QtsRegistrationUpdatedPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = Contact.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { preImageContact },
                FilteringAttributes = new string[]
                {
                    Contact.Fields.dfeta_qtlsdate
                }
            });
    }

    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IPluginExecutionContext>();
        var orgService = serviceProvider.GetRequiredService<IOrganizationService>();

        if (context.PostEntityImages.ContainsKey("PostContactImage"))
        {
            //when triggered from contact update
            Entity entity = (Entity)context.InputParameters["Target"];
            var qtsDates = this.GetContactQTSDates(orgService, entity.Id).ToList();
            var qtlsDate = entity.GetAttributeValue<DateTime?>(Contact.Fields.dfeta_qtlsdate);
            qtsDates.Add(qtlsDate);
            var qtsDate = qtsDates.Where(x => x != null).OrderBy(date => date).FirstOrDefault();
            UpdatePersonQtsDate(orgService, entity.Id, qtsDate);
        }
        else if (context.PreEntityImages.TryGetValue("preImage", out var preImage))
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
                            //qtsdate is the earliest of all qtsregistrations & contact.qtlsDate
                            var qtsDates = this.GetContactQTSDates(orgService, personId).ToList();
                            var qtlsDate = this.GetContactQTLSDate(orgService, personId);
                            qtsDates.Add(qtlsDate);
                            var earliestQTSDate = qtsDates.Where(x => x != null).OrderBy(date => date).FirstOrDefault();
                            UpdatePersonQtsDate(orgService, personId, earliestQTSDate);
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

    public DateTime? GetContactQTLSDate(IOrganizationService orgService, Guid personId)
    {
        var entity = orgService.Retrieve(Contact.EntityLogicalName, personId, new ColumnSet(new string[] {
                Contact.Fields.dfeta_qtlsdate
            }));

        if (entity != null)
        {
            var qtlsDate = entity.GetAttributeValue<DateTime?>(Contact.Fields.dfeta_qtlsdate);
            return qtlsDate;
        }
        return null;
    }

    private DateTime?[] GetContactQTSDates(IOrganizationService orgService, Guid? personId)
    {
        QueryExpression query = new QueryExpression();
        query.EntityName = dfeta_qtsregistration.EntityLogicalName;
        query.ColumnSet = new ColumnSet(new string[] {
                dfeta_qtsregistration.Fields.dfeta_QTSDate
            });
        query.Criteria = new FilterExpression(LogicalOperator.And);
        query.Criteria.AddCondition(dfeta_qtsregistration.Fields.dfeta_PersonId, ConditionOperator.Equal, personId);
        query.Criteria.AddCondition(dfeta_qtsregistration.Fields.StateCode, ConditionOperator.Equal, 0/*Active*/);

        EntityCollection collection = orgService.RetrieveMultiple(query);
        if (collection != null && collection.Entities != null && collection.Entities.Count > 0)
        {
            return collection.Entities.Select(x => x.Contains(dfeta_qtsregistration.Fields.dfeta_QTSDate) ? x[dfeta_qtsregistration.Fields.dfeta_QTSDate] as DateTime? : null).ToArray();
        }
        return Array.Empty<DateTime?>();
    }

    private void UpdatePersonQtsDate(IOrganizationService orgService, Guid personId, DateTime? qtsDate)
    {
        orgService.Execute(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = personId,
                dfeta_QTSDate = qtsDate
            },
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
