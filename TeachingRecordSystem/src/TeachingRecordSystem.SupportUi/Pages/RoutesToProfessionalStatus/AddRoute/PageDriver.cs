using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public static class PageDriver
{
    public static RoutePage NextPage(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status, RoutePage currentPage, bool fromCheckAnswers, AddRouteState state)
    {
        if (fromCheckAnswers)
        {
            return RoutePage.CheckYourAnswers;
        }

        var pagesInOrder = Enum.GetValues(typeof(RoutePage))
            .Cast<RoutePage>()
            .Where(p => p > currentPage)
            .OrderBy(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.FieldRequirementForPage(route, status);

            if (pageRequired != FieldRequirement.NotApplicable && (!page.WasPreviouslyCompleted(state) || page.WasChangedThisPass(state)))
            {
                // if the route has an implicit exemption, don't show the induction exemption page
                if (page == RoutePage.InductionExemption
                    && route.InductionExemptionReason is not null
                    && route.InductionExemptionReason.RouteImplicitExemption)
                {
                    continue;
                }
                else
                {
                    return page;
                }
            }
        }

        return RoutePage.CheckYourAnswers;
    }

    public static RoutePage PreviousPage(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status, RoutePage currentPage, bool fromCheckAnswers, AddRouteState state)
    {
        if (fromCheckAnswers)
        {
            return RoutePage.CheckYourAnswers;
        }

        var pagesInOrder = Enum.GetValues(typeof(RoutePage))
            .Cast<RoutePage>()
            .Where(p => p < currentPage)
            .OrderByDescending(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.FieldRequirementForPage(route, status);

            if (pageRequired != FieldRequirement.NotApplicable && (!page.WasPreviouslyCompleted(state) || page.WasChangedThisPass(state)))
            {
                // if the route has an implicit exemption, don't show the induction exemption page
                if (page == RoutePage.InductionExemption
                    && route.InductionExemptionReason is not null
                    && route.InductionExemptionReason.RouteImplicitExemption)
                {
                    continue;
                }
                else
                {
                    return page;
                }
            }
        }

        return RoutePage.Route;
    }

    //public static bool IsLastPage(AddRoutePage currentPage)
    //{
    //    var lastPage = Enum.GetValues(typeof(AddRoutePage))
    //        .Cast<AddRoutePage>()
    //        .OrderByDescending(p => p)
    //        .First();

    //    return lastPage == currentPage;
    //}

    public static FieldRequirement FieldRequirementForPage(this RoutePage page, RouteToProfessionalStatusType Route, RouteToProfessionalStatusStatus Status)
    {
        return page switch
        {
            RoutePage.Route => FieldRequirement.Mandatory,
            RoutePage.Status => FieldRequirement.Mandatory,
            RoutePage.StartAndEndDate => QuestionDriverHelper.FieldRequired(Route.TrainingEndDateRequired, Status.GetEndDateRequirement()),
            RoutePage.HoldsFrom => QuestionDriverHelper.FieldRequired(Route.HoldsFromRequired, Status.GetHoldsFromRequirement()),
            RoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(Route.InductionExemptionRequired, Status.GetInductionExemptionRequirement()),
            RoutePage.TrainingProvider => QuestionDriverHelper.FieldRequired(Route.TrainingProviderRequired, Status.GetTrainingProviderRequirement()),
            RoutePage.DegreeType => QuestionDriverHelper.FieldRequired(Route.DegreeTypeRequired, Status.GetDegreeTypeRequirement()),
            RoutePage.Country => QuestionDriverHelper.FieldRequired(Route.TrainingCountryRequired, Status.GetCountryRequirement()),
            RoutePage.AgeRangeSpecialism => QuestionDriverHelper.FieldRequired(Route.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement()),
            RoutePage.SubjectSpecialisms => QuestionDriverHelper.FieldRequired(Route.TrainingSubjectsRequired, Status.GetSubjectsRequirement()),
            RoutePage.ChangeReason => FieldRequirement.Mandatory,
            RoutePage.CheckYourAnswers => FieldRequirement.Mandatory,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }

    public static bool WasPreviouslyCompleted(this RoutePage page, AddRouteState state)
    {
        return page switch
        {
            RoutePage.Route =>
                state.RouteToProfessionalStatusId != null,
            RoutePage.Status =>
                state.Status != null,
            RoutePage.StartAndEndDate =>
                state.TrainingStartDate != null ||
                state.TrainingEndDate != null,
            RoutePage.HoldsFrom =>
                state.HoldsFrom != null,
            RoutePage.InductionExemption =>
                state.IsExemptFromInduction != null,
            RoutePage.TrainingProvider =>
                state.TrainingProviderId != null,
            RoutePage.DegreeType =>
                state.DegreeTypeId != null,
            RoutePage.Country =>
                state.TrainingCountryId != null,
            RoutePage.AgeRangeSpecialism =>
                state.TrainingAgeSpecialismType != null ||
                state.TrainingAgeSpecialismRangeFrom != null ||
                state.TrainingAgeSpecialismRangeTo != null,
            RoutePage.SubjectSpecialisms =>
                state.TrainingSubjectIds.Any(),
            RoutePage.ChangeReason =>
                state.ChangeReason != null ||
                state.ChangeReasonDetail.ChangeReasonDetail != null ||
                state.ChangeReasonDetail.HasAdditionalReasonDetail != null ||
                state.ChangeReasonDetail.UploadEvidence != null ||
                state.ChangeReasonDetail.EvidenceFileId != null,
            RoutePage.CheckYourAnswers => false,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }

    public static bool WasChangedThisPass(this RoutePage page, AddRouteState state)
    {
        return page switch
        {
            RoutePage.Route =>
                state.NewRouteToProfessionalStatusId != state.RouteToProfessionalStatusId,
            RoutePage.Status =>
                state.NewStatus != state.Status,
            RoutePage.StartAndEndDate =>
                state.NewTrainingStartDate != state.TrainingStartDate ||
                state.NewTrainingEndDate != state.TrainingEndDate,
            RoutePage.HoldsFrom =>
                state.NewHoldsFrom != state.HoldsFrom,
            RoutePage.InductionExemption =>
                state.NewIsExemptFromInduction != state.IsExemptFromInduction,
            RoutePage.TrainingProvider =>
                state.NewTrainingProviderId != state.TrainingProviderId,
            RoutePage.DegreeType =>
                state.NewDegreeTypeId != state.DegreeTypeId,
            RoutePage.Country =>
                state.NewTrainingCountryId != state.TrainingCountryId,
            RoutePage.AgeRangeSpecialism =>
                state.NewTrainingAgeSpecialismType != state.TrainingAgeSpecialismType ||
                state.NewTrainingAgeSpecialismRangeFrom != state.TrainingAgeSpecialismRangeFrom ||
                state.NewTrainingAgeSpecialismRangeTo != state.TrainingAgeSpecialismRangeTo,
            RoutePage.SubjectSpecialisms =>
                !new HashSet<Guid>(state.NewTrainingSubjectIds).SetEquals(state.TrainingSubjectIds),
            RoutePage.ChangeReason =>
                state.NewChangeReason != state.ChangeReason ||
                state.NewChangeReasonDetail.ChangeReasonDetail != state.ChangeReasonDetail.ChangeReasonDetail ||
                state.NewChangeReasonDetail.HasAdditionalReasonDetail != state.ChangeReasonDetail.HasAdditionalReasonDetail ||
                state.NewChangeReasonDetail.UploadEvidence != state.ChangeReasonDetail.UploadEvidence ||
                state.NewChangeReasonDetail.EvidenceFileId != state.ChangeReasonDetail.EvidenceFileId,
            RoutePage.CheckYourAnswers => false,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }
}
