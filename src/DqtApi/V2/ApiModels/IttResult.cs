using System.ComponentModel;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.V2.ApiModels
{
    public enum IttResult
    {
        [Description("Application Received")]
        ApplicationReceived = 389040008,

        [Description("Application Unsuccessful")]
        ApplicationUnsuccessful = 389040009,

        [Description("Approved")]
        Approved = 389040004,

        [Description("Deferred")]
        Deferred = 389040005,

        [Description("Deferred for Skills Tests")]
        DeferredforSkillsTests = 389040006,

        [Description("Fail")]
        Fail = 389040001,

        [Description("Info")]
        Info = 389040010,

        [Description("In Training")]
        InTraining = 389040003,

        [Description("No Result Submitted")]
        NoResultSubmitted = 389040011,

        [Description("Pass")]
        Pass = 389040000,

        [Description("Under Assessment")]
        UnderAssessment = 389040007,

        [Description("Withdrawn")]
        Withdrawn = 389040002,
    }

    public static class IttResultExtensions
    {
        public static dfeta_ITTResult ConvertToITTResult(this IttResult input) =>
            input.ConvertToEnum<IttResult, dfeta_ITTResult>();

        public static bool TryConvertToConvertToITTResult(this IttResult input, out dfeta_ITTResult result) =>
            input.TryConvertToEnum(out result);
    }
}
