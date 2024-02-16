using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure.FakeXrmEasy.Plugins;

public class UpdateQtlsDateSetPlugin : IPlugin
{
    internal static void Register(IXrmFakedContext context)
    {
        context.RegisterPluginStep<UpdateQtlsDateSetPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = Contact.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation,
                FilteringAttributes = new string[]
                {
                    Contact.Fields.dfeta_qtlsdate
                }
            });

        context.RegisterPluginStep<UpdateQtlsDateSetPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = Contact.EntityLogicalName,
                MessageName = "Update",
                Stage = ProcessingStepStage.Preoperation,
                FilteringAttributes = new string[]
                {
                            Contact.Fields.dfeta_qtlsdate
                }
            });
    }

    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IPluginExecutionContext>();

        //Update or create
        Entity target = (Entity)context!.InputParameters["Target"];
        if (target.TryGetAttributeValue<DateTime?>(Contact.Fields.dfeta_qtlsdate, out DateTime? qtlsDate))
        {
            if (qtlsDate.HasValue)
            {
                target[Contact.Fields.dfeta_QtlsDateHasBeenSet] = true;
            }
        }
    }
}
