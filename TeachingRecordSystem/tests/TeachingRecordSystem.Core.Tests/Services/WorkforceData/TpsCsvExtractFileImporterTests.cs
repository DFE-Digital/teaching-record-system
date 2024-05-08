using System.Text;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Tests.Services.WorkforceData;

public class TpsCsvExtractFileImporterTests(DbFixture dbFixture)
{
    public static TheoryData<TpsCsvExtractFileImportTestScenarioData> GetImportFileTestScenarioData()
    {
        var validFormatTrn = "1234567";
        var invalidFormatTrn = "12345678";
        var validFormatNationalInsuranceNumber = "QQ123456U";
        var invalidFormatNationalInsuranceNumber = "1234";
        var validFormatDateOfBirth = "01/01/1980";
        var invalidFormatDateOfBirth = "1234";
        var validFormatDateOfDeath = "01/02/2024";
        var invalidFormatDateOfDeath = "1234";
        var validFormatLocalAuthorityCode = "123";
        var invalidFormatLocalAuthorityCode = "1234";
        var validFormatEstablishmentNumber = "1234";
        var invalidFormatEstablishmentNumber = "12345";
        var validFormatEmploymentStartDate = "03/02/2023";
        var invalidFormatEmploymentStartDate = "1234";
        var validFormatEmploymentEndDate = "03/05/2024";
        var invalidFormatEmploymentEndDate = "1234";
        var validFullOrPartTimeIndicator = "PTI";
        var invalidFullOrPartTimeIndicator = "PTI1";
        var validWithdrawlIndicator = "W";
        var invalidWithdrawlIndicator = "E1";
        var validFormatExtractDate = "07/03/2024";
        var invalidFormatExtractDate = "1234";
        var validFormatGender = "Male";
        var invalidFormatGender = "None";

        return new()
        {
            // Null TRN
            new ()
            {
                Row = new()
                {
                    Trn = null,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.TrnIncorrectFormat,
            },
            // Invalid TRN
            new ()
            {
                Row = new()
                {
                    Trn = invalidFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.TrnIncorrectFormat,
            },
            // Null National Insurance Number
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = null,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.NationalInsuranceNumberIncorrectFormat,
            },
            // Invalid National Insurance Number
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = invalidFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.NationalInsuranceNumberIncorrectFormat,
            },
            // Null Date of Birth
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = null,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.DateOfBirthIncorrectFormat,
            },
            // Invalid Date of Birth
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = invalidFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.DateOfBirthIncorrectFormat,
            },
            // Null Date of Death
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = null,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.None,
            },
            // Invalid Date of Death
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = invalidFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.DateOfDeathIncorrectFormat,
            },
            // Null Local Authority Code
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = null,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.LocalAuthorityCodeIncorrectFormat,
            },
            // Invalid Local Authority Code
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = invalidFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.LocalAuthorityCodeIncorrectFormat,
            },
            // Null Establishment Number
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = null,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.None,
            },
            // Invalid Establishment Number
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = invalidFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.EstablishmentNumberIncorrectFormat,
            },
            // Null Employment Start Date
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = null,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.EmploymentStartDateIncorrectFormat,
            },
            // Invalid Employment Start Date
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = invalidFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.EmploymentStartDateIncorrectFormat,
            },
            // Null Employment End Date
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = null,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.EmploymentEndDateIncorrectFormat,
            },
            // Invalid Employment End Date
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = invalidFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.EmploymentEndDateIncorrectFormat,
            },
            // Null Full or Part Time Indicator
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = null,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.FullOrPartTimeIndicatorIncorrectFormat,
            },
            // Invalid Full or Part Time Indicator
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = invalidFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.FullOrPartTimeIndicatorIncorrectFormat,
            },
            // Null Withdrawl Indicator
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = null,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.None,
            },
            // Invalid Withdrawl Indicator
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = invalidWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.WithdrawlIndicatorIncorrectFormat,
            },
            // Null Extract Date
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = null,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.ExtractDateIncorrectFormat,
            },
            // Invalid Extract Date
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = invalidFormatExtractDate,
                    Gender = validFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.ExtractDateIncorrectFormat,
            },
            // Null Gender
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = null
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.GenderIncorrectFormat,
            },
            // Invalid Gender
            new ()
            {
                Row = new()
                {
                    Trn = validFormatTrn,
                    NationalInsuranceNumber = validFormatNationalInsuranceNumber,
                    DateOfBirth = validFormatDateOfBirth,
                    DateOfDeath = validFormatDateOfDeath,
                    MemberPostcode = null,
                    MemberEmailAddress = null,
                    LocalAuthorityCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EstablishmentPostcode = null,
                    EstablishmentEmailAddress = null,
                    EmploymentStartDate = validFormatEmploymentStartDate,
                    EmploymentEndDate = validFormatEmploymentEndDate,
                    FullOrPartTimeIndicator = validFullOrPartTimeIndicator,
                    WithdrawlIndicator = validWithdrawlIndicator,
                    ExtractDate = validFormatExtractDate,
                    Gender = invalidFormatGender
                },
                ExpectedResult = TpsCsvExtractItemLoadErrors.GenderIncorrectFormat,
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetImportFileTestScenarioData))]
    public async Task ImportFile_WithRowData_InsertsRecordWithExpectedResult(TpsCsvExtractFileImportTestScenarioData testScenarioData)
    {
        // Arrange
        var tpsExtractStorageService = Mock.Of<ITpsExtractStorageService>();
        var dbContextFactory = dbFixture.GetDbContextFactory();
        var clock = new TestableClock();
        var tpsCsvExtractId = Guid.NewGuid();
        var filename = "pending/test.csv";
        var csvContent = new StringBuilder();
        csvContent.AppendLine("Teachers Pensions Reference Number (TRN),National Insurance Number (NINO),Date of Birth (DOB),Date of Death,Postcode,Email Address (Member),Local Authority Code,Establishment Code,Postcode (Establishment),Email Address (Establishment),Start Date,End Date,Full Time / Part Time Indicator,Withdrawal Indicator,Extract Date,Gender");
        csvContent.AppendLine($"{testScenarioData.Row.Trn},{testScenarioData.Row.NationalInsuranceNumber},{testScenarioData.Row.DateOfBirth},{testScenarioData.Row.DateOfDeath},{testScenarioData.Row.MemberPostcode},{testScenarioData.Row.MemberEmailAddress},{testScenarioData.Row.LocalAuthorityCode},{testScenarioData.Row.EstablishmentCode},{testScenarioData.Row.EstablishmentPostcode},{testScenarioData.Row.EstablishmentEmailAddress},{testScenarioData.Row.EmploymentStartDate},{testScenarioData.Row.EmploymentEndDate},{testScenarioData.Row.FullOrPartTimeIndicator},{testScenarioData.Row.WithdrawlIndicator},{testScenarioData.Row.ExtractDate},{testScenarioData.Row.Gender}");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));
        Mock.Get(tpsExtractStorageService)
            .Setup(x => x.GetFile(filename, CancellationToken.None))
            .ReturnsAsync(stream);

        // Act
        var importer = new TpsCsvExtractFileImporter(
            tpsExtractStorageService,
            dbContextFactory,
            clock);
        await importer.ImportFile(tpsCsvExtractId, filename, CancellationToken.None);

        // Assert
        using var dbContext = dbContextFactory.CreateDbContext();
        var result = await dbContext.TpsCsvExtractLoadItems
            .Where(x => x.TpsCsvExtractId == tpsCsvExtractId)
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal(testScenarioData.ExpectedResult, result.First().Errors);

        await dbContext.TpsCsvExtracts.Where(x => x.TpsCsvExtractId == tpsCsvExtractId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task CopyValidFormatDataToStaging_WithValidData_InsertsRecordWithExpectedResult()
    {
        // Arrange
        var tpsExtractStorageService = Mock.Of<ITpsExtractStorageService>();
        var dbContextFactory = dbFixture.GetDbContextFactory();
        var clock = new TestableClock();
        var tpsCsvExtractId = Guid.NewGuid();
        var tpsCsvExtract = new TpsCsvExtract
        {
            TpsCsvExtractId = tpsCsvExtractId,
            Filename = "pending/test.csv",
            CreatedOn = clock.UtcNow
        };

        var validLoadItem = new TpsCsvExtractLoadItem
        {
            TpsCsvExtractLoadItemId = Guid.NewGuid(),
            TpsCsvExtractId = tpsCsvExtractId,
            Trn = "1234567",
            NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
            DateOfBirth = "01/01/1980",
            DateOfDeath = "01/02/2024",
            MemberPostcode = null,
            MemberEmailAddress = null,
            LocalAuthorityCode = "123",
            EstablishmentNumber = "1234",
            EstablishmentPostcode = null,
            EstablishmentEmailAddress = null,
            MemberId = null,
            EmploymentStartDate = "03/02/2023",
            EmploymentEndDate = "03/05/2024",
            FullOrPartTimeIndicator = "PTI",
            WithdrawlIndicator = null,
            ExtractDate = "07/03/2024",
            Gender = "Male",
            Created = clock.UtcNow,
            Errors = TpsCsvExtractItemLoadErrors.None
        };
        var invalidLoadItem = new TpsCsvExtractLoadItem
        {
            TpsCsvExtractLoadItemId = Guid.NewGuid(),
            TpsCsvExtractId = tpsCsvExtractId,
            Trn = "7654321",
            NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
            DateOfBirth = "01/01/1980",
            DateOfDeath = "01/02/2024",
            MemberPostcode = null,
            MemberEmailAddress = null,
            LocalAuthorityCode = "123",
            EstablishmentNumber = "1234",
            EstablishmentPostcode = null,
            EstablishmentEmailAddress = null,
            MemberId = null,
            EmploymentStartDate = "03/02/2023",
            EmploymentEndDate = "03/05/2024",
            FullOrPartTimeIndicator = "PTI",
            WithdrawlIndicator = null,
            ExtractDate = "07/03/2024",
            Gender = "Male",
            Created = clock.UtcNow,
            Errors = TpsCsvExtractItemLoadErrors.GenderIncorrectFormat
        };

        using var dbContext = dbContextFactory.CreateDbContext();
        await dbContext.TpsCsvExtracts.AddAsync(tpsCsvExtract);
        await dbContext.TpsCsvExtractLoadItems.AddRangeAsync(validLoadItem, invalidLoadItem);
        await dbContext.SaveChangesAsync();

        // Act
        var importer = new TpsCsvExtractFileImporter(
            tpsExtractStorageService,
            dbContextFactory,
            clock);
        await importer.CopyValidFormatDataToStaging(tpsCsvExtractId, CancellationToken.None);

        // Assert
        var result = await dbContext.TpsCsvExtractItems
            .Where(x => x.TpsCsvExtractId == tpsCsvExtractId)
            .ToListAsync();
        Assert.Single(result);
        Assert.Equal(validLoadItem.TpsCsvExtractLoadItemId, result.First().TpsCsvExtractLoadItemId);
    }
}

public class TpsCsvExtractFileImportTestScenarioData
{
    public required TpsCsvExtractRowRaw Row { get; init; }
    public required TpsCsvExtractItemLoadErrors ExpectedResult { get; init; }
}
