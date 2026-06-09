using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class GetPersonTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = new GetPersonCommand(
            Trn: "0000000",
            Include: GetPersonCommandIncludes.None,
            DateOfBirth: null,
            NationalInsuranceNumber: null);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Fact]
    public async Task HandleAsync_PersonExists_ReturnsResult()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var command = new GetPersonCommand(
            Trn: person.Trn,
            Include: GetPersonCommandIncludes.None,
            DateOfBirth: null,
            NationalInsuranceNumber: null);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.Equal(person.Trn, success.Trn);
        Assert.Equal(person.FirstName, success.FirstName);
        Assert.Equal(person.LastName, success.LastName);
        Assert.Equal(person.DateOfBirth, success.DateOfBirth);
    }

    [Fact]
    public async Task HandleAsync_PersonExistsButDateOfBirthDoesNotMatch_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithDateOfBirth(new DateOnly(1990, 6, 15)));

        var command = new GetPersonCommand(
            Trn: person.Trn,
            Include: GetPersonCommandIncludes.None,
            DateOfBirth: new DateOnly(1991, 1, 1),
            NationalInsuranceNumber: null);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Fact]
    public async Task HandleAsync_PersonExistsButNationalInsuranceNumberDoesNotMatch_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber("AA112233A"));

        var command = new GetPersonCommand(
            Trn: person.Trn,
            Include: GetPersonCommandIncludes.None,
            DateOfBirth: null,
            NationalInsuranceNumber: "ZZ999999Z");

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }
}
