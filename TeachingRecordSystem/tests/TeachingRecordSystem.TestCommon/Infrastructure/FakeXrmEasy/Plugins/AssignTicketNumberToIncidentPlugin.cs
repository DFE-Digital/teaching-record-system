using System.Security.Cryptography;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure.FakeXrmEasy.Plugins;

public class AssignTicketNumberToIncidentPlugin : IPlugin
{
    private static int _lastTicketNumber = 10000;

    internal static void Register(IXrmFakedContext context)
    {
        context.RegisterPluginStep<AssignTicketNumberToIncidentPlugin>(
            new PluginStepDefinition()
            {
                EntityLogicalName = Incident.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation
            });
    }

    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IPluginExecutionContext>();
        var incident = (Incident)context.InputParameters["Target"];

        var ticketNumber = GenerateTicketNumber();
        incident.Attributes.Add(Incident.Fields.TicketNumber, ticketNumber);

        static string GenerateTicketNumber()
        {
            // Real TicketNumbers look like CAS-01587-Y9W0D7

            var numberSegment = Interlocked.Increment(ref _lastTicketNumber);

            var randomSegmentBuffer = new byte[3].AsSpan();
            RandomNumberGenerator.Fill(randomSegmentBuffer);
            var randomSegment = Convert.ToHexString(randomSegmentBuffer).ToString();

            return $"CAS-{numberSegment}-{randomSegment}";
        }
    }
}
