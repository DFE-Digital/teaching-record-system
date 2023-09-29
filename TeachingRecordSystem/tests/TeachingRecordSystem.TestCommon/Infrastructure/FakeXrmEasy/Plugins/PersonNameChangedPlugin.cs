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

public class PersonNameChangedPlugin : IPlugin
{
    internal static void Register(IXrmFakedContext context)
    {
        var preImageDefinition = new PluginImageDefinition(
            "preImage",
            ProcessingStepImageType.PreImage,
            new string[]
            {
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_StatedFirstName,
                Contact.Fields.dfeta_StatedMiddleName,
                Contact.Fields.dfeta_StatedLastName
            });
        context.RegisterPluginStep<PersonNameChangedPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = Contact.EntityLogicalName,
                MessageName = "Update",
                Stage = ProcessingStepStage.Preoperation,
                ImagesDefinitions = new List<IPluginImageDefinition>() { preImageDefinition }
            });
    }

    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IPluginExecutionContext>();
        var target = context.InputParameters["Target"] as Entity;
        var preImage = context.PreEntityImages["preImage"] as Entity;
        bool clearStaleStatedNames = false;

        if (target is not null && (target.Contains(Contact.Fields.FirstName) || target.Contains(Contact.Fields.MiddleName) || target.Contains(Contact.Fields.LastName)))
        {
            var orgService = serviceProvider.GetRequiredService<IOrganizationService>();

            if (target.Contains(Contact.Fields.FirstName))
            {
                string name = preImage.GetAttributeValue<string>(Contact.Fields.FirstName);
                string newName = target.GetAttributeValue<string>(Contact.Fields.FirstName);

                if (string.Compare(name, newName, false) == 0)
                {
                    target.Attributes.Remove(Contact.Fields.FirstName);
                }
                else
                {
                    CreatePreviousName(orgService, target.Id, name, dfeta_NameType.FirstName);
                    if (!target.Contains(Contact.Fields.dfeta_StatedFirstName))
                    {
                        clearStaleStatedNames = true;
                    }
                }
            }

            // middle name
            if (target.Contains(Contact.Fields.MiddleName))
            {
                string name = preImage.GetAttributeValue<string>(Contact.Fields.MiddleName);
                string newName = target.GetAttributeValue<string>(Contact.Fields.MiddleName);

                if (string.Compare(name, newName, false) == 0)
                {
                    target.Attributes.Remove(Contact.Fields.MiddleName);
                }
                else
                {
                    CreatePreviousName(orgService, target.Id, name, dfeta_NameType.MiddleName);
                    if (!target.Contains(Contact.Fields.dfeta_StatedMiddleName))
                    {
                        clearStaleStatedNames = true;
                    }
                }
            }

            // last name
            if (target.Contains(Contact.Fields.LastName))
            {
                string name = preImage.GetAttributeValue<string>(Contact.Fields.LastName);
                string newName = target.GetAttributeValue<string>(Contact.Fields.LastName);

                if (string.Compare(name, newName, false) == 0)
                {
                    target.Attributes.Remove(Contact.Fields.LastName);
                }
                else
                {
                    target[Contact.Fields.dfeta_PreviousLastName] = name;

                    CreatePreviousName(orgService, target.Id, name, dfeta_NameType.LastName);
                    if (!target.Contains(Contact.Fields.dfeta_StatedLastName))
                    {
                        clearStaleStatedNames = true;
                    }
                }
            }

            if (clearStaleStatedNames)
            {
                target[Contact.Fields.dfeta_StatedFirstName] = null;
                target[Contact.Fields.dfeta_StatedMiddleName] = null;
                target[Contact.Fields.dfeta_StatedLastName] = null;
            }
        }
    }

    private static void CreatePreviousName(IOrganizationService orgService, Guid contactId, string name, dfeta_NameType type)
    {
        orgService.Execute(new CreateRequest()
        {
            Target = new dfeta_previousname()
            {
                dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, contactId),
                dfeta_name = name,
                dfeta_Type = type,
                dfeta_ChangedOn = DateTime.Now
            }
        });
    }
}
