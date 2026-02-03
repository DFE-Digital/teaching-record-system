namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public class MergePersonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task MergePerson_PersonsMatchOnAllFields()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(person1.FirstName)
            .WithMiddleName(person1.MiddleName)
            .WithLastName(person1.LastName)
            .WithDateOfBirth(person1.DateOfBirth)
            .WithEmailAddress(person1.EmailAddress)
            .WithNationalInsuranceNumber(person1.NationalInsuranceNumber!));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnMergePersonEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMatchesPageAsync(person1.PersonId);
        // select Person 2 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person2.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMergePageAsync(person1.PersonId);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonCheckAnswersPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Confirm and update primary record");

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
        await page.AssertBannerAsync("Success", $"Records merged for {StringExtensions.JoinNonEmpty(' ', person1.FirstName, person1.MiddleName, person1.LastName)}");

        await page.AssertBannerLinksToPersonRecord(person2.PersonId);
    }

    [Fact]
    public async Task MergePerson_PersonsDifferOnAllFields()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnMergePersonEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMergePageAsync(person1.PersonId);
        await page.ClickContinueButtonAsync();

        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.EmailAddress!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person2.FirstName, "First name");
        await page.AssertContentEqualsAsync(person2.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person2.LastName, "Last name");
        await page.AssertContentEqualsAsync(person2.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person2.EmailAddress!, "Email");
        await page.AssertContentEqualsAsync(person2.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickButtonAsync("Confirm and update primary record");

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
        await page.AssertBannerAsync("Success", $"Records merged for {person2.FirstName} {person2.MiddleName} {person2.LastName}");

        await page.AssertBannerLinksToPersonRecord(person1.PersonId);
    }

    [Fact]
    public async Task MergePerson_NavigateBack()
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnMergePersonEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMergePageAsync(person1.PersonId);
        await page.ClickContinueButtonAsync();

        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.EmailAddress!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person2.FirstName, "First name");
        await page.AssertContentEqualsAsync(person2.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person2.LastName, "Last name");
        await page.AssertContentEqualsAsync(person2.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person2.EmailAddress!, "Email");
        await page.AssertContentEqualsAsync(person2.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickBackLinkAsync();

        await page.AssertOnMergePersonMergePageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnMergePersonMatchesPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnMergePersonEnterTrnPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
    }


    [Theory]
    [InlineData("change-firstname-link")]
    [InlineData("change-middlename-link")]
    [InlineData("change-lastname-link")]
    [InlineData("change-dob-link")]
    [InlineData("change-email-link")]
    [InlineData("change-gender-link")]
    public async Task MergePerson_CYA_ClickChangeLink_RedirectsToMergePage(string testIdSelector)
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()).WithGender(Gender.Male));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()).WithGender(Gender.Female));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnMergePersonEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMergePageAsync(person1.PersonId);
        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.EmailAddress!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.ClickRadioByLabelAsync(person2.Gender.ToString()!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person2.FirstName, "First name");
        await page.AssertContentEqualsAsync(person2.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person2.LastName, "Last name");
        await page.AssertContentEqualsAsync(person2.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person2.EmailAddress!, "Email");
        await page.AssertContentEqualsAsync(person2.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickLinkForElementWithTestIdAsync(testIdSelector);
        await page.AssertOnMergePersonMergePageAsync(person1.PersonId);
    }

    [Theory]
    [InlineData("change-firstname-link")]
    [InlineData("change-middlename-link")]
    [InlineData("change-lastname-link")]
    [InlineData("change-dob-link")]
    [InlineData("change-email-link")]
    [InlineData("change-ni-link")]
    [InlineData("change-evidence-link")]
    [InlineData("change-comments-link")]
    public async Task MergePerson_CYA_ChangeDetails_ContinuesToCYA(string changeLink)
    {
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person1.PersonId);
        await page.ClickButtonAsync("Merge with another record");

        await page.AssertOnMergePersonEnterTrnPageAsync(person1.PersonId);
        await page.FillAsync("label:text-is('Enter the TRN of the other record you want to merge')", person2.Trn);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMatchesPageAsync(person1.PersonId);
        // select Person 1 as primary record
        await page.ClickRadioByLabelAsync($"TRN {person1.Trn}");
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonMergePageAsync(person1.PersonId);
        await page.ClickRadioByLabelAsync(person2.FirstName);
        await page.ClickRadioByLabelAsync(person2.MiddleName);
        await page.ClickRadioByLabelAsync(person2.LastName);
        await page.ClickRadioByLabelAsync(person2.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person2.EmailAddress!);
        await page.ClickRadioByLabelAsync(person2.NationalInsuranceNumber!);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person2.FirstName, "First name");
        await page.AssertContentEqualsAsync(person2.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person2.LastName, "Last name");
        await page.AssertContentEqualsAsync(person2.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person2.EmailAddress!, "Email");
        await page.AssertContentEqualsAsync(person2.NationalInsuranceNumber!, "National Insurance number");


        await page.ClickLinkForElementWithTestIdAsync(changeLink);

        await page.AssertOnMergePersonMergePageAsync(person1.PersonId);
        await page.ClickRadioByLabelAsync(person1.FirstName);
        await page.ClickRadioByLabelAsync(person1.MiddleName);
        await page.ClickRadioByLabelAsync(person1.LastName);
        await page.ClickRadioByLabelAsync(person1.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(person1.EmailAddress!);
        await page.ClickRadioByLabelAsync(person1.NationalInsuranceNumber!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePersonCheckAnswersPageAsync(person1.PersonId);
        await page.AssertContentEqualsAsync(person1.FirstName, "First name");
        await page.AssertContentEqualsAsync(person1.MiddleName, "Middle name");
        await page.AssertContentEqualsAsync(person1.LastName, "Last name");
        await page.AssertContentEqualsAsync(person1.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(person1.EmailAddress!, "Email");
        await page.AssertContentEqualsAsync(person1.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickBackLinkAsync();

        await page.AssertOnMergePersonMergePageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnMergePersonMatchesPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnMergePersonEnterTrnPageAsync(person1.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person1.PersonId);
    }
}
