using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure.FakeXrmEasy.Plugins;

public class UpdateInductionStatusPlugin : IPlugin
{
    internal static void Register(IXrmFakedContext context)
    {
        var inductionStatusPostImage = new PluginImageDefinition(
            "PostInductionImage",
            ProcessingStepImageType.PostImage,
            new string[]
            {
                dfeta_induction.Fields.dfeta_PersonId,
                dfeta_induction.Fields.dfeta_InductionStatus,
            });

        var inductionStatusPreImage = new PluginImageDefinition(
            "PreInductionImage",
            ProcessingStepImageType.PreImage,
            new string[]
            {
                dfeta_induction.Fields.dfeta_PersonId,
                dfeta_induction.Fields.dfeta_InductionStatus,
            });

        var contactPostImage = new PluginImageDefinition(
            "PostImageContactImage",
            ProcessingStepImageType.PostImage,
            new string[]
            {
                Contact.Fields.ContactId,
                Contact.Fields.dfeta_qtlsdate,
            });

        context.RegisterPluginStep<UpdateInductionStatusPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = dfeta_induction.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { inductionStatusPostImage },
                FilteringAttributes = new string[]
                {
                    dfeta_induction.Fields.dfeta_InductionStatus
                }
            });
        context.RegisterPluginStep<UpdateInductionStatusPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = dfeta_induction.EntityLogicalName,
                MessageName = "Delete",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { inductionStatusPreImage }
            });

        context.RegisterPluginStep<UpdateInductionStatusPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = Contact.EntityLogicalName,
                MessageName = "Update",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { contactPostImage },
                FilteringAttributes = new string[]
                {
                    Contact.Fields.dfeta_qtlsdate
                }
            });


        context.RegisterPluginStep<UpdateInductionStatusPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = Contact.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { contactPostImage },
                FilteringAttributes = new string[]
                {
                            Contact.Fields.dfeta_qtlsdate
                }
            });

        context.RegisterPluginStep<UpdateInductionStatusPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = dfeta_induction.EntityLogicalName,
                MessageName = "Update",
                Stage = ProcessingStepStage.Postoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { inductionStatusPreImage },
                FilteringAttributes = new string[]
                {
                    dfeta_induction.Fields.dfeta_InductionStatus,
                    dfeta_induction.Fields.StateCode
                }
            });
    }


    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IPluginExecutionContext>();
        var orgService = serviceProvider.GetRequiredService<IOrganizationService>();

        if (context.PreEntityImages != null && context.PreEntityImages.Contains("PreInductionImage"))
        {
            Entity preImageEnt = context.PreEntityImages["PreInductionImage"];
            if (preImageEnt.Contains(dfeta_induction.Fields.dfeta_PersonId))
            {
                if (preImageEnt[dfeta_induction.Fields.dfeta_PersonId] != null)
                {
                    Guid personId = ((EntityReference)preImageEnt[dfeta_induction.Fields.dfeta_PersonId]).Id;
                    if (context.MessageName == "Delete")
                    {
                        //triggered from inductionstatus deleted - needs to recompute induction status
                        var qtlsDate = GetContactQTLSDate(orgService, personId);
                        OptionSetValue? derivedInductionStatus = CalculateInductionStation(qtlsDate, null);
                        if (derivedInductionStatus != null)
                        {
                            SetIndutionStatus(orgService, personId, derivedInductionStatus?.Value);
                        }
                        else
                        {
                            SetIndutionStatus(orgService, personId, null);
                        }
                        return;
                    }

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity entity = (Entity)context.InputParameters["Target"];
                        if (entity.Attributes.Contains("statecode"))
                        {
                            var state = entity.GetAttributeValue<OptionSetValue>("statecode");
                            if (state?.Value == 1)
                            {
                                SetIndutionStatus(orgService, personId, null);
                            }
                        }
                        else
                        {
                            //triggered from update to InductionStatus.InductionStatus
                            var qtlsDate = GetContactQTLSDate(orgService, personId);
                            var existingInductionStatus = entity.GetAttributeValue<OptionSetValue?>(dfeta_induction.Fields.dfeta_InductionStatus);
                            if (existingInductionStatus != null)
                            {
                                var derivedInductionStatus = this.CalculateInductionStation(qtlsDate, existingInductionStatus);
                                SetIndutionStatus(orgService, personId, derivedInductionStatus?.Value);
                            }
                        }
                        return;
                    }
                }
            }
        }
        else if (context.PostEntityImages != null && context.PostEntityImages.Contains("PostInductionImage"))
        {
            //triggered on newly created inductionstatus
            Entity postImageEnt = context.PostEntityImages["PostInductionImage"];
            Guid personId = ((EntityReference)postImageEnt[dfeta_induction.Fields.dfeta_PersonId]).Id;
            var qtlsDate = GetContactQTLSDate(orgService, personId);
            var existingInductionStatus = postImageEnt.GetAttributeValue<OptionSetValue>(dfeta_induction.Fields.dfeta_InductionStatus);
            var derivedInductionStatus = CalculateInductionStation(qtlsDate, existingInductionStatus);
            SetIndutionStatus(orgService, personId, derivedInductionStatus?.Value);
            return;
        }
        else if (context.PostEntityImages != null && context.PostEntityImages.Contains("PostImageContactImage"))
        {
            //Triggered on updates to contact.dfeta_qtlsdate
            Entity postImageEnt = context.PostEntityImages["PostImageContactImage"];
            if (postImageEnt.Contains(Contact.Fields.Id))
            {
                var personId = postImageEnt.GetAttributeValue<Guid>(Contact.Fields.Id);
                var qtlsDate = postImageEnt.GetAttributeValue<DateTime?>(Contact.Fields.dfeta_qtlsdate);
                var inductionStatus = this.GetContactInductionStatus(orgService, personId);
                OptionSetValue? derivedInductionStatus = this.CalculateInductionStation(qtlsDate, inductionStatus);
                if (derivedInductionStatus != null)
                {
                    SetIndutionStatus(orgService, personId, derivedInductionStatus?.Value);
                }
                else
                {
                    SetIndutionStatus(orgService, personId, null);
                }
            }
            return;
        }


    }

    private OptionSetValue? GetContactInductionStatus(IOrganizationService orgService, Guid personId)
    {
        QueryExpression query = new QueryExpression();
        query.EntityName = dfeta_induction.EntityLogicalName;
        query.ColumnSet = new ColumnSet(new string[] {
                dfeta_induction.Fields.dfeta_InductionStatus
            });
        query.Criteria = new FilterExpression(LogicalOperator.And);
        query.Criteria.AddCondition(dfeta_induction.Fields.dfeta_PersonId, ConditionOperator.Equal, personId);
        query.Criteria.AddCondition(dfeta_induction.Fields.StateCode, ConditionOperator.Equal, 0/*Active*/);

        EntityCollection collection = orgService.RetrieveMultiple(query);
        if (collection != null && collection.Entities != null && collection.Entities.Count > 0)
        {
            return collection.Entities.Select(x => x.GetAttributeValue<OptionSetValue>(dfeta_induction.Fields.dfeta_InductionStatus)).FirstOrDefault();
        }

        return null;
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
    public static void SetIndutionStatus(IOrganizationService orgService, Guid id, int? indutionStatus)
    {
        if (orgService == null)
        {
            throw new ArgumentNullException("orgService");
        }

        Entity person = new Entity(Contact.EntityLogicalName);
        person.Id = id;
        person.Attributes.Add(Contact.Fields.dfeta_InductionStatus, indutionStatus.HasValue ? new OptionSetValue(indutionStatus.Value) : null);
        orgService.Update(person);
    }

    private OptionSetValue? CalculateInductionStation(DateTime? qtlsDate, OptionSetValue? value)
    {
        if (value != null)
        {
            switch (value?.Value)
            {
                case (int)dfeta_InductionStatus.Exempt:
                case (int)dfeta_InductionStatus.FailedinWales when qtlsDate.HasValue:
                case (int)dfeta_InductionStatus.InProgress when qtlsDate.HasValue:
                case (int)dfeta_InductionStatus.InductionExtended when qtlsDate.HasValue:
                case (int)dfeta_InductionStatus.NotYetCompleted when qtlsDate.HasValue:
                case (int)dfeta_InductionStatus.RequiredtoComplete when qtlsDate.HasValue:
                    {
                        return new OptionSetValue((int)dfeta_InductionStatus.Exempt);
                    }
                default:
                    {
                        return value;
                    }
            }
        }
        else
        {
            if (qtlsDate.HasValue)
            {
                return new OptionSetValue((int)dfeta_InductionStatus.Exempt);
            }
            return null;
        }
    }
}
