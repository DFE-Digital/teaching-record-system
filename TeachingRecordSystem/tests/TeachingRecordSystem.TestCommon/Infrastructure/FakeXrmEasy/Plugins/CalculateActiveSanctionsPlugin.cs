using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure.FakeXrmEasy.Plugins;

public class CalculateActiveSanctionsPlugin : IPlugin
{
    internal static void Register(IXrmFakedContext context)
    {
        context.RegisterPluginStep<CalculateActiveSanctionsPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = dfeta_sanction.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Postoperation
            });
        context.RegisterPluginStep<CalculateActiveSanctionsPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = dfeta_sanction.EntityLogicalName,
                MessageName = "Update",
                Stage = ProcessingStepStage.Postoperation
            });
    }

    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IPluginExecutionContext>();
        var target = context.InputParameters["Target"] as Entity;

        if (target is not null && target.Contains(dfeta_sanction.Fields.dfeta_PersonId))
        {
            var orgService = serviceProvider.GetRequiredService<IOrganizationService>();
            var personId = target.GetAttributeValue<EntityReference>(dfeta_sanction.Fields.dfeta_PersonId).Id;
            var activeSanctions = orgService.RetrieveMultiple(new QueryExpression(dfeta_sanction.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(dfeta_sanction.PrimaryIdAttribute),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression(dfeta_sanction.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_sanctionState.Active),
                        new ConditionExpression(dfeta_sanction.Fields.dfeta_PersonId, ConditionOperator.Equal, personId),
                        new ConditionExpression(dfeta_sanction.Fields.dfeta_Spent, ConditionOperator.NotEqual, true)
                    }
                }
            });

            orgService.Execute(new UpdateRequest()
            {
                Target = new Contact()
                {
                    Id = personId,
                    dfeta_ActiveSanctions = (activeSanctions.Entities.Count > 0)
                }
            });
        }
    }
}
