global using FluentValidation;
global using GovUk.Frontend.AspNetCore;
global using GovUk.Questions.AspNetCore;
global using TeachingRecordSystem.WebCommon;
global using TeachingRecordSystem.WebCommon.FormFlow;
// GovUk.Questions and FormFlow both define Journey/JourneyInstanceId. While we migrate off
// FormFlow, resolve the unqualified names to GovUk.Questions so new code needs no qualification;
// remaining FormFlow usages are fully qualified. These aliases are intentionally kept even while
// unreferenced, so IDE0005 (unnecessary using) is suppressed for them.
#pragma warning disable IDE0005
global using JourneyAttribute = GovUk.Questions.AspNetCore.JourneyAttribute;
global using JourneyInstanceId = GovUk.Questions.AspNetCore.JourneyInstanceId;
#pragma warning restore IDE0005
global using UiDefaults = TeachingRecordSystem.SupportUi.Pages.Common.UiDefaults;
global using WebConstants = TeachingRecordSystem.WebCommon.WebConstants;
