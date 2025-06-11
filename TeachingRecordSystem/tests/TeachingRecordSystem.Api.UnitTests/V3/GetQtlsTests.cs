using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public partial class GetQtlsTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public Task HandleAsync_PersonDoesNotExist_ReturnsNotFoundError() =>
        WithHandler<GetQtlsHandler>(async handler =>
        {
            // Arrange
            var command = new GetQtlsCommand("0000000");

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.PersonNotFound);
        });

    [Fact]
    public Task HandleAsync_PersonDoesNotHaveQtlsRoute_ReturnsNullQtsDate() =>
        WithHandler<GetQtlsHandler>(async handler =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync(p => p.WithTrn());

            var command = new GetQtlsCommand(person.Trn!);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);
            Assert.Equal(person.Trn, success.Trn);
            Assert.Null(success.QtsDate);
        });

    [Fact]
    public Task HandleAsync_PersonHasQtlsRoute_ReturnsAwardedDate() =>
        WithHandler<GetQtlsHandler>(async handler =>
        {
            // Arrange
            var qtlsDate = new DateOnly(2025, 5, 1);

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithRouteToProfessionalStatus(s => s
                    .WithRoute(RouteToProfessionalStatusType.QtlsAndSetMembershipId)
                    .WithStatus(RouteToProfessionalStatusStatus.Holds)
                    .WithHoldsFrom(qtlsDate)));

            var command = new GetQtlsCommand(person.Trn!);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);
            Assert.Equal(person.Trn, success.Trn);
            Assert.Equal(qtlsDate, success.QtsDate);
        });
}
