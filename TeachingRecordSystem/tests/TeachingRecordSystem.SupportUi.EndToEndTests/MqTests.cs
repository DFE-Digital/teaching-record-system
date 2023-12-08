using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class MqTests : TestBase
{
    public MqTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task AddMQ()
    {
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var specialism = await TestData.ReferenceDataCache.GetMqSpecialismByValue("Hearing");
        var startDate = new DateOnly(2021, 3, 1);
        var result = dfeta_qualification_dfeta_MQ_Status.Passed;
        var endDate = new DateOnly(2021, 11, 5);
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPage(personId);

        await page.AssertOnPersonQualificationsPage(personId);

        await page.ClickButton("Add a mandatory qualification");

        await page.AssertOnAddMqProviderPage();

        await page.CheckAsync($"label:text-is('{mqEstablishment.dfeta_name}')");

        await page.ClickContinueButton();

        await page.AssertOnAddMqSpecialismPage();

        await page.CheckAsync($"label:text-is('{specialism.dfeta_name}')");

        await page.ClickContinueButton();

        await page.AssertOnAddMqStartDatePage();

        await page.FillDateInput(startDate);

        await page.ClickContinueButton();

        await page.AssertOnAddMqResultPage();

        await page.CheckAsync($"label:text-is('{result}')");

        await page.FillDateInput(endDate);

        await page.ClickContinueButton();

        await page.AssertOnAddMqCheckAnswersPage();

        await page.ClickButton("Confirm mandatory qualification");

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification added");
    }
}
