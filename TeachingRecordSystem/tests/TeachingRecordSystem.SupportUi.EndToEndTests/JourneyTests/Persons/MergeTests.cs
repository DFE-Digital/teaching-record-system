using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public class MergeTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Merge_PersonsMatchOnAllFields()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(person1.FirstName)
            .WithMiddleName(person1.MiddleName)
            .WithLastName(person1.LastName)
            .WithDateOfBirth(person1.DateOfBirth)
            .WithEmail(person1.Email)
            .WithNationalInsuranceNumber(person1.NationalInsuranceNumber!));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        // select Person 2 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person2.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Confirm and update primary record");

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
        await page.AssertBannerAsync("Success", $"Records merged for {StringHelper.JoinNonEmpty(' ', person1.FirstName, person1.MiddleName, person1.LastName)}");

        await page.FollowBannerLink("View record (opens in a new tab)");
        await page.AssertOnPersonDetailPageAsync(person2.PersonId);
    }

    [Test]
    public async Task Merge_PersonsDifferOnAllFields()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickContinueButtonAsync();

        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.Email!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person2.FirstName, "First name");
        await page.AssertContentEqualsAsync(person2.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person2.LastName, "Last name");
        await page.AssertContentEqualsAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person2.Email!, "Email");
        await page.AssertContentEqualsAsync(person2.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickButtonAsync("Confirm and update primary record");

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
        await page.AssertBannerAsync("Success", $"Records merged for {person2.FirstName} {person2.MiddleName} {person2.LastName}");

        await page.FollowBannerLink("View record (opens in a new tab)");
        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
    }

    [Test]
    public async Task Merge_NavigateBack()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickContinueButtonAsync();

        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.Email!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person2.FirstName, "First name");
        await page.AssertContentEqualsAsync(person2.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person2.LastName, "Last name");
        await page.AssertContentEqualsAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person2.Email!, "Email");
        await page.AssertContentEqualsAsync(person2.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
    }

    [Test]
    public async Task Merge_CYA_ChangePrimaryPerson_NavigatesBackToCYA()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickContinueButtonAsync();

        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.Email!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-primary-person-link");

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
    }

    [Test]
    public async Task Merge_CYA_ChangeDetails_NavigatesBackToCYA()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickContinueButtonAsync();

        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.Email!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
    }

    [Test]
    public async Task Merge_CYA_ChangePrimaryPerson_ContinuesToCYA()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.Email!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person2.FirstName, "First name");
        await page.AssertContentEqualsAsync(person2.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person2.LastName, "Last name");
        await page.AssertContentEqualsAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person2.Email!, "Email");
        await page.AssertContentEqualsAsync(person2.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickLinkForElementWithTestIdAsync("change-primary-person-link");

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        // select Person 2 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person2.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person2.FirstName, "First name");
        await page.AssertContentEqualsAsync(person2.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person2.LastName, "Last name");
        await page.AssertContentEqualsAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person2.Email!, "Email");
        await page.AssertContentEqualsAsync(person2.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
    }
    [Test]
    public async Task Merge_CYA_ChangeDetails_ContinuesToCYA()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.Email!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person2.FirstName, "First name");
        await page.AssertContentEqualsAsync(person2.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person2.LastName, "Last name");
        await page.AssertContentEqualsAsync(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person2.Email!, "Email");
        await page.AssertContentEqualsAsync(person2.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickRadioByLabelAsync(person1.FirstName);
        await page.ClickRadioByLabelAsync(person1.MiddleName);
        await page.ClickRadioByLabelAsync(person1.LastName);
        await page.ClickRadioByLabelAsync(person1.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person1.Email!);
        await page.ClickRadioByLabelAsync(person1.NationalInsuranceNumber!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonMergeCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person1.FirstName, "First name");
        await page.AssertContentEqualsAsync(person1.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person1.LastName, "Last name");
        await page.AssertContentEqualsAsync(person1.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person1.Email!, "Email");
        await page.AssertContentEqualsAsync(person1.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMergePageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeMatchesPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonMergeEnterTrnPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
    }
}
